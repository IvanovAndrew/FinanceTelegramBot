using Domain;
using MediatR;

namespace Application.Statistic.StatisticBalance;

public class BalanceStatisticCalculatedEvent : INotification
{
    public long SessionId { get; init; }
    public int? LastSentMessageId { get; init; }
    public Money Saldo { get; init; }
    public Money DailyBudget { get; init; }
    public Money MandatoryExpenses { get; init; }
    public IReadOnlyList<MonthlyBalance> MonthBalances { get; init; }
    public Currency Currency { get; init; }
    public MonthRange MonthRange { get; init; }
    public FinancialPeriod PeriodLeft { get; init; }
}