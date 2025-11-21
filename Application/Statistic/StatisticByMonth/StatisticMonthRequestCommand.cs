using MediatR;

namespace Application.Statistic.StatisticByMonth;

public record StatisticMonthRequestCommand : IRequest
{
    public long SessionId { get; init; }
}