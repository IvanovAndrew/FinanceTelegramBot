using MediatR;

namespace Application.Commands.StatisticBySubcategoryByMonth;

public class StatisticBySubcategoryByMonthCommand : IRequest
{
    public long SessionId { get; init; }
}