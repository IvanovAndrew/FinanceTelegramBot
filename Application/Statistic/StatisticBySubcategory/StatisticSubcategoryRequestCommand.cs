using MediatR;

namespace Application.Statistic.StatisticBySubcategory;

public record StatisticSubcategoryRequestCommand : IRequest
{
    public long SessionId { get; init; }
}