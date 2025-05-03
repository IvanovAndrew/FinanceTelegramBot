using MediatR;

namespace Application.Statistic.StatisticBySubcategory;

public class StatisticBySubcategorySaveDateCommand : IRequest
{
    public long SessionId { get; init; }
    public string DateFromText { get; init; }
}