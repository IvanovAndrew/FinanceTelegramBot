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
        logger.LogInformation($"{nameof(SpendingUntilPaydayCommandHandler)} started");
        
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

        var salaryDay = GetSalaryDay(dateTimeService.Today());

        var diff = salaryDay.DayNumber - today.DayNumber;
        var moneyPerDay = BudgetPlanner.Plan(request.Balance, diff, missingOutcomes);
        
        logger.LogInformation($"{nameof(SpendingUntilPaydayCommandHandler)} finished");

        return new MoneyLeft(){MoneyPerDay = moneyPerDay, Payday = salaryDay};
    }

    private DateOnly GetSalaryDay(DateOnly today)
    {
        var firstWorkingDay = dateTimeService.FirstWorkingDayOfMonth(today);

        return today < firstWorkingDay ? firstWorkingDay : dateTimeService.FirstWorkingDayOfMonth(today.AddMonths(1));
    }
}