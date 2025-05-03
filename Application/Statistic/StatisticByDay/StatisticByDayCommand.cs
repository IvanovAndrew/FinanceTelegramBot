using MediatR;

namespace Application.Commands.StatisticByDay;

public class StatisticByDayCommand : IRequest
{
    public long SessionId { get; init; }
}