using Application.Bot.Flows;
using MediatR;

namespace Application.Core.Statistic;

public record StatisticDayRequestCommand : IRequest
{
    public long SessionId { get; init; }

    public StatisticsQuery Query { get; init; }
}