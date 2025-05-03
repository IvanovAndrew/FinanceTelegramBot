using MediatR;

namespace Application.Statistic.StatisticByCategory;

public class StatisticByCategoryCommand : IRequest
{
    public long SessionId { get; init; }
}