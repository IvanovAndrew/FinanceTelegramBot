using Domain;
using MediatR;

namespace Application.Statistic.StatisticByMonth;

public record MonthOutcomesReadEvent : INotification
{
    public long SessionId { get; init; }
    public int LastSentMessageId { get; init; }
    
    public YearMonth Month { get; init; }
    
    public IReadOnlyList<Outcome> Outcomes { get; init; }
}