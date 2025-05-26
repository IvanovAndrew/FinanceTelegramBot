using MediatR;

namespace Application.Commands.StatisticBySubcategoryByMonth;

public class StatisticSubcategoryMonthRequestCommand : IRequest
{
    public long SessionId { get; init; }
}