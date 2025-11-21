using Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Statistic.StatisticBalance;

public record SpendingUntilPaydayCommand : IRequest<MoneyLeft>
{
    public long SessionId { get; init; }
    public DateOnly DateFrom { get; init; }
    public Currency Currency { get; init; }
    public Money Balance { get; init; }
}

public record struct MoneyLeft
{
    public Money MoneyPerDay { get; init; }
    public DateOnly Payday { get; init; }
}

public class SpendingUntilPaydayCommandHandler(IDateTimeService dateTimeService, IFinanceRepository repository, ILogger<SpendingUntilPaydayCommandHandler> logger) : IRequestHandler<SpendingUntilPaydayCommand, MoneyLeft>
{
    public async Task<MoneyLeft> Handle(SpendingUntilPaydayCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation($"{nameof(SpendingUntilPaydayCommandHandler)} started {request}");

        var today = dateTimeService.Today();
        var monthAgo = today.AddMonths(-1);
        var theFirstDayOfPreviousMonth = new DateOnly(monthAgo.Year, monthAgo.Month, 1);
        
        var outcomes = 
            await repository.ReadOutcomes(new FinanceFilter()
            {
                Currency = request.Currency,
                Income = false,
                DateFrom = theFirstDayOfPreviousMonth,
                DateTo = today
            }, cancellationToken);

        var previousMonth = new HashSet<IMoneyTransfer>();
        var currentMonth = new HashSet<IMoneyTransfer>();

        bool hasExpensesToday = false;
        
        foreach (var outcome in outcomes)
        {
            if (outcome.SubCategory?.IsRecurringMonthly is true)
            {
                if (outcome.Date.Year == monthAgo.Year && outcome.Date.Month == monthAgo.Month)
                {
                    previousMonth.Add(outcome);
                }
                else if (outcome.Date.Year == today.Year && outcome.Date.Month == today.Month)
                {
                    if (outcome.Date == today)
                    {
                        hasExpensesToday = true;
                    }
                    
                    currentMonth.Add(outcome);
                }
            }
        }

        var missingOutcomes = new List<Money>();
        foreach (var outcome in previousMonth)
        {
            var exists = currentMonth.Any(c => c.Category == outcome.Category && c.SubCategory == outcome.SubCategory);
            if (!exists)
            {
                missingOutcomes.Add(outcome.Amount);
            }
        }

        var incomes = await repository.ReadIncomes(new FinanceFilter()
        {
            Currency = request.Currency,
            Income = false,
            DateFrom = theFirstDayOfPreviousMonth,
            DateTo = today
        }, cancellationToken);

        var salaryDay = GetSalaryDay(today, incomes.Where(i => i.Category == Category.FromString("Зарплата")).Max(c => c.Date));

        var moneyPerDay = BudgetPlanner.Plan(request.Balance, today, salaryDay, missingOutcomes, hasExpensesToday);
        
        logger.LogInformation($"{nameof(SpendingUntilPaydayCommandHandler)} finished");

        return new MoneyLeft(){MoneyPerDay = moneyPerDay, Payday = salaryDay};
    }

    private DateOnly GetSalaryDay(DateOnly today, DateOnly? lastSalaryDay)
    {
        DateOnly firstWorkingDay;
        if (lastSalaryDay == null)
        {
            firstWorkingDay = dateTimeService.FirstWorkingDayOfMonth(today.AddMonths(1));
            return today < firstWorkingDay ? firstWorkingDay : dateTimeService.FirstWorkingDayOfMonth(today.AddMonths(1));
        }
        
        return dateTimeService.FirstWorkingDayOfMonth(lastSalaryDay.Value.AddMonths(1));
    }
}