using Application.Core;
using MediatR;

namespace Application.Bot.Statistic;

public record StatisticSubcategoryMonthRequestCommand : IRequest
{
    public long SessionId { get; init; }
    public StatisticsQuery Query { get; init; }
}