using MediatR;

namespace Domain.Events;

public class MoneyTransferReadDomainEvent<T> : INotification
{
    public long SessionId { get; init; }
    public Statistic<T> Statistic { get; init; }
    public DateOnly? DateFrom { get; init; }
    public DateOnly? DateTo { get; init; }
    public string? Subtitle { get; init; }
    public string? FirstColumnName { get; init; }
}