using MediatR;

namespace Application.Commands.StatisticByDay;

public class StatisticDayRequestCommand : IRequest
{
    public long SessionId { get; init; }
}