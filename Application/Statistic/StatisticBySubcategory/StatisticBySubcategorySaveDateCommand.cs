using MediatR;

namespace Application.Statistic.StatisticBySubcategory;

public record StatisticBySubcategorySaveDateCommand : IRequest
{
    public long SessionId { get; init; }
    public string DateFromText { get; init; }
}