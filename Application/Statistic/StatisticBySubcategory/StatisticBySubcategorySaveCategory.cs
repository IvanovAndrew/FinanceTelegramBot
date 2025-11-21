using MediatR;

namespace Application.Statistic.StatisticBySubcategory;

public record StatisticBySubcategorySaveCategoryCommand : IRequest
{
    public long SessionId { get; init; }
    public string Category { get; init; }
}