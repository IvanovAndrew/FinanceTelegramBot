using Domain;
using MediatR;

namespace Application.AddMoneyTransfer;

public class MoneyTransferReadDomainEvent : INotification
{
    public long SessionId { get; init; }
    public StatisticWrapper Statistic { get; init; }
    public DateOnly? DateFrom { get; init; }
    public DateOnly? DateTo { get; init; }
    public string? Subtitle { get; init; }
    public string? FirstColumnName { get; init; }
}

