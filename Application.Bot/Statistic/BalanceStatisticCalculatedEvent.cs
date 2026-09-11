using Domain;
using Domain.Services;
using MediatR;

namespace Application.Bot.Statistic;

public class BalanceStatisticCalculatedEvent : INotification
{
    public long SessionId { get; init; }
    public Money Saldo { get; init; }
    public Money DailyBudget { get; init; }
    public IReadOnlyCollection<MissingRecurringExpense> FutureExpenses { get; init; }
    public IReadOnlyList<MonthlyBalance> MonthBalances { get; init; }
    public Currency Currency { get; init; }
    public MonthRange MonthRange { get; init; }
    public FinancialPeriod PeriodLeft { get; init; }
}