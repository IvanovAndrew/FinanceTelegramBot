using MediatR;

namespace Application.Statistic.StatisticByCategory;

public class StatisticByCategoryRequestCommand : IRequest
{
    public long SessionId { get; init; }
}