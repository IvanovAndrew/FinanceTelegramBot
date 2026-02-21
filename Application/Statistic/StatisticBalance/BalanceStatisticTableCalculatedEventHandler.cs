using System.Text;
using Domain;
using MediatR;

namespace Application.Statistic.StatisticBalance;

public class BalanceStatisticTableCalculatedEventHandler(IMessageService messageService) : INotificationHandler<BalanceStatisticCalculatedEvent>
{
    public async Task Handle(BalanceStatisticCalculatedEvent notification, CancellationToken cancellationToken)
    {
        var zero = Money.Zero(notification.Currency);
        var totalBalance = notification.MonthBalances.Aggregate(new Balance(zero, zero), (acc, monthBalance) => acc + monthBalance.Balance);
        
        var table = BuildTable(totalBalance, notification.MonthRange.From, notification.Currency);
        
        await messageService.EditSentTextMessageAsync(
            new Message()
            {
                ChatId = notification.SessionId,
                Id = notification.LastSentMessageId,
                Table = table
            }, cancellationToken);
    }
    
    private static Table BuildTable(Balance balance, YearMonth monthFrom, Currency currency)
    {
        var table = new Table()
        {
            Title = "Balance",
            Subtitle = $"From {monthFrom.ToString(DateFormat.FullMonthName)}",
            FirstColumnName = "Balance",
            Currencies = [currency],
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

public class BalanceStatisticPaydayCalculatedEventHandler(IMessageService messageService) : INotificationHandler<BalanceStatisticCalculatedEvent>
{
    public async Task Handle(BalanceStatisticCalculatedEvent notification, CancellationToken cancellationToken)
    {
        var text = Text(notification.Currency, notification.MonthBalances, notification.MandatoryExpenses, notification.PeriodLeft, notification.DailyBudget);

        await messageService.SendTextMessageAsync(
            new Message()
            {
                ChatId = notification.SessionId,
                Text = text,
                UseMarkdown = true,
            }, cancellationToken);
    }

    private string Text(Currency currency, IReadOnlyList<MonthlyBalance> monthBalances, Money mandatoryExpenses, FinancialPeriod period, Money dailyBudget)
    {
        var zero = Money.Zero(currency);
        var totalBalance = monthBalances.Aggregate(new Balance(zero, zero), (acc, monthBalance) => acc + monthBalance.Balance);

        var dayFormat = "d MMMM yyyy";
        var text = new StringBuilder()
            .AppendLine("Information")
            .AppendLine($"Total balance: {totalBalance.Saldo}")
            .AppendLine($"Future expenses: {mandatoryExpenses}")
            .AppendLine($"Real free money: {totalBalance.Saldo - mandatoryExpenses}")
            .AppendLine("")
            .AppendLine($"Today is {period.Start.ToString(dayFormat)}")
            .AppendLine($"The payday is {period.End.ToString(dayFormat)}")
            .AppendLine($"Days till the payday (today {(period.IncludeStart ? "is" : "isn't")} included): {period.DaysRemaining}")
            .AppendLine()
            .AppendLine($"{dailyBudget} can be spent daily till the payday")
            .ToString();
        return text;
    }
}