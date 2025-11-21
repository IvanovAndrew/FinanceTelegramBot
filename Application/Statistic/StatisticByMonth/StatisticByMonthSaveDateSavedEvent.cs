using MediatR;

namespace Application.Statistic.StatisticByMonth;

public record StatisticByMonthSaveDateSavedEvent : INotification
{
    public long SessionId { get; init; }
}