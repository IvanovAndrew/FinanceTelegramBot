using System.Text;
using Application.Core;
using Application.Core.Statistic;
using Domain;
using Domain.Services;
using MediatR;

namespace Application.Bot.Statistic;

public class BalanceStatisticPaydayCalculatedEventHandler(IConversation conversation)
    : INotificationHandler<BalanceStatisticCalculatedEvent>
{
    public async Task Handle(BalanceStatisticCalculatedEvent notification, CancellationToken cancellationToken)
    {
        var text = Text(notification.Currency, notification.MonthBalances, notification.FutureExpenses,
            notification.PeriodLeft, notification.DailyBudget);

        await conversation.Update(notification.SessionId, Screens.NotifyWithStyle(text), cancellationToken);
    }

    private string Text(Currency currency, IReadOnlyList<MonthlyBalance> monthBalances,
        IReadOnlyCollection<MissingRecurringExpense> futureExpenses, FinancialPeriod period, Money dailyBudget)
    {
        var zero = Money.Zero(currency);
        var totalBalance =
            monthBalances.Aggregate(new Balance(zero, zero), (acc, monthBalance) => acc + monthBalance.Balance);

        var ending = "дней";
        if (period.DaysRemaining % 10 == 1 && period.DaysRemaining != 11)
        {
            ending = "день";
        }
        else if ((period.DaysRemaining % 10 == 2 || period.DaysRemaining % 10 == 3 || period.DaysRemaining % 10 == 4) &&
                 period.DaysRemaining != 12 && period.DaysRemaining != 13 && period.DaysRemaining != 14)
        {
            ending = "дня";
        }
        
        var futureExpensesSum = futureExpenses.Aggregate(zero, (acc, e) => acc + (e.ResolvedAmount ?? zero));

        var text = new StringBuilder()
            .AppendLine("Summary")
            .AppendLine($"From {monthBalances.First().Month.ToString(DateFormat.FullMonthName)}")
            .AppendLine($"Income: {totalBalance.Income}")
            .AppendLine($"Outcome: {totalBalance.Outcome}")
            .AppendLine($"Total balance: {totalBalance.Saldo}")
            .AppendLine($"Future expenses: {string.Join(", ", futureExpensesSum)}")
            .AppendLine($"Safe to spend: {totalBalance.Saldo - futureExpensesSum}")
            .AppendLine("")
            .AppendLine(
                $"📅 {period.Start.ToString(DateFormat.DayWithoutYear)} —— [{period.DaysRemaining} {ending}] —— 💰 {period.End.ToString(DateFormat.DayWithoutYear)}")
            .AppendLine()
            .AppendLine($"{dailyBudget} can be spent daily till the payday")
            .ToString();

        return text;
    }
}