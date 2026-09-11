using MediatR;

namespace Application.Core.Statistic;

public record BalanceStatisticCollectingStarted : INotification
{
    public long SessionId { get; init; }
}