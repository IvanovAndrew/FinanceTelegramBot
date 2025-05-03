using MediatR;

namespace Domain.Events;

public class StatisticByMonthSaveDateSavedEvent : INotification
{
    public long SessionId { get; init; }
}