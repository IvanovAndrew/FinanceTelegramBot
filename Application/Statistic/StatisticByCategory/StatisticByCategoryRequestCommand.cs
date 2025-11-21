using MediatR;

namespace Application.Statistic.StatisticByCategory;

public record StatisticByCategoryRequestCommand : IRequest
{
    public long SessionId { get; init; }
}