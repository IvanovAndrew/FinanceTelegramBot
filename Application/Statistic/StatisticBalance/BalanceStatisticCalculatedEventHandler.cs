using Domain;
using MediatR;

namespace Application.Statistic.StatisticBalance;

public class BalanceStatisticCalculatedEventHandler(IMessageService messageService) : INotificationHandler<BalanceStatisticCalculatedEvent>
{
    public async Task Handle(BalanceStatisticCalculatedEvent notification, CancellationToken cancellationToken)
    {
        var zero = Money.Zero(notification.Currency);
        var totalBalance = notification.MonthBalances.Aggregate(new Balance(zero, zero), (acc, monthBalance) => acc + monthBalance.Balance);
        
        var table = BuildTable(totalBalance, notification.MoneyLeft, notification.DateFrom, notification.SalaryDay, notification.Currency, notification.IncludeToday);
        
        await messageService.EditSentTextMessageAsync(
            new Message()
            {
                ChatId = notification.SessionId,
                Id = notification.LastSentMessageId,
                Table = table
            }, cancellationToken);
    }
    
    private static Table BuildTable(Balance balance, Money moneyLeft, DateOnly dateFrom, DateOnly? salaryDay, Currency currency, bool includeToday)
    {
        string postTableInfo = string.Empty;
        if (salaryDay is { } sd)
        {
            postTableInfo = 
                $"{moneyLeft} can be spent daily till the payday {sd.ToString("d MMMM yyyy")} (today {(includeToday? "is" : "isn't")} included)";
        }
        
        var table = new Table()
        {
            Title = "Balance",
            Subtitle = $"From {dateFrom.ToString("MMMM yyyy")}",
            FirstColumnName = "Balance",
            Currencies = [currency],
            PostTableInfo = postTableInfo
        };
        table.AddRow(new Row()
        {
            FirstColumnValue = "Income",
            CurrencyValues = new Dictionary<Currency, Money>()
                { [currency] = balance.Income }
        });
        table.AddRow(new Row()
        {
            FirstColumnValue = "Outcome",
            CurrencyValues = new Dictionary<Currency, Money>() { [currency] = balance.Outcome }
        });
        table.AddRow(new Row());
        table.AddRow(new Row()
        {
            FirstColumnValue = "Total",
            CurrencyValues = new Dictionary<Currency, Money>()
                { [currency] = balance.Saldo }
        });
        return table;
    }
}