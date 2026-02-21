using MediatR;

namespace Application.Statistic.StatisticByDay;

public record StatisticDayRequestCommand : IRequest
{
    public long SessionId { get; init; }
    public int LastSentMessageId { get; init; }

    public StatisticsQuery Query { get; init; }
    
}