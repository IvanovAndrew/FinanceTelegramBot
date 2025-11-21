using MediatR;

namespace Application.Statistic.StatisticBySubcategoryByMonth;

public record StatisticSubcategoryMonthRequestCommand : IRequest
{
    public long SessionId { get; init; }
}