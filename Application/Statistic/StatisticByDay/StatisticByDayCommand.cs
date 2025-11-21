using MediatR;

namespace Application.Statistic.StatisticByDay;

public record StatisticByDayCommand : IRequest
{
    public long SessionId { get; init; }
}