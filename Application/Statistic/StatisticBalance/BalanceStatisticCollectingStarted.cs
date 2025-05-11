using MediatR;

namespace Application.Statistic.StatisticBalance;

public class BalanceStatisticCollectingStarted : INotification
{
    public long SessionId { get; init; }
    public int? LastSentMessageId { get; init; } 
}