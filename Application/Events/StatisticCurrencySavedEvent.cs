using MediatR;

namespace Application.Events;

public record StatisticCurrencySavedEvent : INotification
{
    public long SessionId { get; init; }
}