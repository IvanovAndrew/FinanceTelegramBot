using MediatR;

namespace Application.Statistic.StatisticBySubcategory;

public class StatisticSubcategoryRequestCommand : IRequest
{
    public long SessionId { get; init; }
}