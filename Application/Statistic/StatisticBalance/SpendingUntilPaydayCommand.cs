using Domain;
using MediatR;

namespace Application.Statistic.StatisticBalance;

public record SpendingUntilPaydayCommand : IRequest<MoneyLeft>
{
    public long SessionId { get; init; }
    public DateOnly DateFrom { get; init; }
    public Currency Currency { get; init; }
    public Money Balance { get; set; }
}

public record MoneyLeft
{
    public Money MoneyPerDay { get; set; }
    public DateOnly Payday { get; set; }
}

public class SpendingUntilPaydayCommandHandler(IDateTimeService dateTimeService, IFinanceRepository repository, IMediator mediator) : IRequestHandler<SpendingUntilPaydayCommand, MoneyLeft>
{
    public async Task<MoneyLeft> Handle(SpendingUntilPaydayCommand request, CancellationToken cancellationToken)
    {
        var monthAgo = dateTimeService.Today().AddMonths(-1);
        var theFirstDayOfPreviousMonth = new DateOnly(monthAgo.Year, monthAgo.Month, 1);
        
        // TODO calculate future mandatory spendings 
        var outcomes = 
        await repository.ReadOutcomes(new FinanceFilter()
        {
            Currency = request.Currency,
            Income = false,
            DateFrom = theFirstDayOfPreviousMonth,
        }, cancellationToken);

        foreach (var outcome in outcomes)
        {
            if (outcome.SubCategory?.IsRecurringMonthly is true)
            {
                
            }
        }
        
        
        var today = dateTimeService.Today();
        var firstWorkingDayOfNextMonth = dateTimeService.GetFirstWorkingDayOfNextMonth();

        var diff = firstWorkingDayOfNextMonth.DayNumber - today.DayNumber;
        var moneyPerDay = BudgetPlanner.Plan(request.Balance, diff, []);

        return new MoneyLeft(){MoneyPerDay = moneyPerDay, Payday = firstWorkingDayOfNextMonth};
    }
}

public class SpendingPerDayCalculatedEventHandler(IMessageService messageService) : INotificationHandler<SpendingPerDayCalculatedEvent>
{
    public async Task Handle(SpendingPerDayCalculatedEvent notification, CancellationToken cancellationToken)
    {
        var text =
            $"{notification.MoneyPerDay} can be spent daily till the payday {notification.ExpectedSalaryDay.ToString("dd M yyyy")}";
        
        await messageService.SendTextMessageAsync(
            new Message()
            {
                ChatId = notification.ChatId,
                Text = text,
            },
            cancellationToken: cancellationToken);
    }
}