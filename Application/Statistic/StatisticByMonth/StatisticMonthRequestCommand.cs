using MediatR;

namespace Application.Commands.StatisticByMonth;

public class StatisticMonthRequestCommand : IRequest
{
    public long SessionId { get; init; }
}