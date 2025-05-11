using MediatR;

namespace Application.Statistic.StatisticBalance;

public class GetBalanceStatisticCommand : IRequest
{
    public long SessionId { get; init; }
    public int? LastSentMessage { get; init; }
}

public class BalanceStatisticCollectingStartedHandler(IMessageService messageService) : INotificationHandler<BalanceStatisticCollectingStarted>
{
    public async Task Handle(BalanceStatisticCollectingStarted notification, CancellationToken cancellationToken)
    {
        await messageService.EditSentTextMessageAsync(
            new Message()
            {
                ChatId = notification.SessionId,
                Id = notification.LastSentMessageId,
                Text = "Loading the incomes and the outcomes... It can take some time"
            }, cancellationToken);
    }
}

public class NeitherIncomesNotOutcomesFoundEvent : INotification
{
    public long SessionId { get; init; }
    public int? LastSentMessageId { get; init; }
}

public class NeitherIncomesNotOutcomesFoundEventHandler(IMessageService messageService) : INotificationHandler<NeitherIncomesNotOutcomesFoundEvent>
{
    public async Task Handle(NeitherIncomesNotOutcomesFoundEvent notification, CancellationToken cancellationToken)
    {
        await messageService.EditSentTextMessageAsync(
            new Message()
            {
                ChatId = notification.SessionId,
                Id = notification.LastSentMessageId,
                Text = "There is no any expenses for this period"
            }, cancellationToken);
    }
}

public class LongOperationCanceledEvent : INotification
{
    public long SessionId { get; init; }
}

public class LongOperationCanceledEventHandler(IMessageService messageService) : INotificationHandler<LongOperationCanceledEvent>
{
    public async Task Handle(LongOperationCanceledEvent notification, CancellationToken cancellationToken)
    {
        await messageService.SendTextMessageAsync(
            new Message()
            {
                ChatId = notification.SessionId,
                Text = "Operation has been canceled"
            }, cancellationToken
        );
    }
}

public class BalanceStatisticCalculatedEvent : INotification
{
    public long SessionId { get; init; }
    public int? LastSentMessageId { get; init; }
    public Table Table { get; init; }
}

public class BalanceStatisticCalculatedEventHandler(IMessageService messageService) : INotificationHandler<BalanceStatisticCalculatedEvent>
{
    public async Task Handle(BalanceStatisticCalculatedEvent notification, CancellationToken cancellationToken)
    {
        await messageService.EditSentTextMessageAsync(
            new Message()
            {
                ChatId = notification.SessionId,
                Id = notification.LastSentMessageId,
                Table = notification.Table
            }, cancellationToken);
    }
}