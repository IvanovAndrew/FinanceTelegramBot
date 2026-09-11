using Application.Bot.Flows;
using Application.Core;
using Application.Core.Statistic;
using MediatR;

namespace Application.Bot.Statistic;

public record GetBalanceStatisticCommand : IRequest
{
    public long SessionId { get; init; }
    public StatisticsQuery Query { get; init; }
}

public class BalanceStatisticCollectingStartedHandler(IConversation conversation) : INotificationHandler<BalanceStatisticCollectingStarted>
{
    public async Task Handle(BalanceStatisticCollectingStarted notification, CancellationToken cancellationToken)
    {
        await conversation.Update(
            notification.SessionId, 
            Screens.NotifyOperationInProgress($"Loading the incomes and the outcomes...{Environment.NewLine}It can take some time"), cancellationToken);
    }
}