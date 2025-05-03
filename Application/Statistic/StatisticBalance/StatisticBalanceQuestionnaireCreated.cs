using MediatR;

namespace Application.Statistic.StatisticBalance;

public class StatisticBalanceQuestionnaireCreated : INotification
{
    public long SessionId { get; init; }
    public int LastSentMessageId { get; init; }
}