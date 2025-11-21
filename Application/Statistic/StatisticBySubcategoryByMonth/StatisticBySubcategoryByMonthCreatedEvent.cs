using MediatR;

namespace Application.Statistic.StatisticBySubcategoryByMonth;

public record StatisticBySubcategoryByMonthCreatedEvent : INotification
{
    public long SessionId { get; init; }
}