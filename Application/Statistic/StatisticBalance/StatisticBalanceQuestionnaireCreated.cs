using MediatR;

namespace Application.Statistic.StatisticBalance;

public record StatisticBalanceQuestionnaireCreated : INotification
{
    public long SessionId { get; init; }
    public int LastSentMessageId { get; init; }
}