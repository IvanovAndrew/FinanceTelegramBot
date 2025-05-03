using MediatR;

namespace Domain.Events;

public class StatisticByDayDateSavedEvent : INotification
{
    public long SessionId { get; init; }
}