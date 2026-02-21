using MediatR;

namespace Application.Statistic.StatisticBySubcategoryByMonth;

public record StatisticBySubcategoryByMonthCommand : IRequest
{
    public long SessionId { get; init; }
    public int LastSentMessageId { get; init; }
    public StatisticsQuery Query { get; init; }
}