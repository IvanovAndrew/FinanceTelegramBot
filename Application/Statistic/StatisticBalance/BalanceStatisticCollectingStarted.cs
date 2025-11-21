using MediatR;

namespace Application.Statistic.StatisticBalance;

public record BalanceStatisticCollectingStarted : INotification
{
    public long SessionId { get; init; }
    public int? LastSentMessageId { get; init; } 
}