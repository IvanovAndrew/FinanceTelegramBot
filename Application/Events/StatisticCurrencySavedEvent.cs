using MediatR;

namespace Application.Events;

public class StatisticCurrencySavedEvent : INotification
{
    public long SessionId { get; init; }
}