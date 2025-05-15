using MediatR;

namespace Application.Statistic.StatisticBySubcategoryByMonth;

public class StatisticSubcategoryMonthRequestCommand : IRequest
{
    public long SessionId { get; init; }
}