using MediatR;

namespace Application.Statistic.StatisticBySubcategory;

public record StatisticBySubcategoryCommand : IRequest
{
    public long SessionId { get; init; }
}