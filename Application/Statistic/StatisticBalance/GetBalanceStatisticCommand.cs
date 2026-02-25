using MediatR;

namespace Application.Statistic.StatisticBalance;

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
            Screens.NotifyLoading($"Loading the incomes and the outcomes...{Environment.NewLine}It can take some time"), cancellationToken);
    }
}

public class NeitherIncomesNotOutcomesFoundEvent : INotification
{
    public long SessionId { get; init; }
}

public class NeitherIncomesNotOutcomesFoundEventHandler(IConversation conversation) : INotificationHandler<NeitherIncomesNotOutcomesFoundEvent>
{
    public async Task Handle(NeitherIncomesNotOutcomesFoundEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionId, Screens.Notify("There is no any expenses for this period"), cancellationToken);
    }
}

public class LongOperationCanceledEvent : INotification
{
    public long SessionId { get; init; }
}

public class LongOperationCanceledEventHandler(IConversation conversation) : INotificationHandler<LongOperationCanceledEvent>
{
    public async Task Handle(LongOperationCanceledEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionId, Screens.Notify("Operation has been canceled"),
            cancellationToken);
    }
}