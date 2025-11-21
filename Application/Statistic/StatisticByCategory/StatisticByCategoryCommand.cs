using MediatR;

namespace Application.Statistic.StatisticByCategory;

public record StatisticByCategoryCommand : IRequest
{
    public long SessionId { get; init; }
}