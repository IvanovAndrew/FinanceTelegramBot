using Domain;
using MediatR;

namespace Application.Statistic.StatisticBalance;

public class BalanceStatisticCalculatedEvent : INotification
{
    public long SessionId { get; init; }
    public int? LastSentMessageId { get; init; }
    public Money MoneyLeft { get; init; }
    public IReadOnlyList<MonthlyBalance> MonthBalances { get; init; }
    public Currency Currency { get; init; }
    public DateOnly DateFrom { get; init; }
    public DateOnly? SalaryDay { get; init; }
    public bool IncludeToday { get; init; }
}