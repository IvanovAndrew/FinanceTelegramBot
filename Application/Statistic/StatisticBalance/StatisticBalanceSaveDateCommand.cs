using MediatR;

namespace Application.Statistic.StatisticBalance;

public record StatisticBalanceSaveDateCommand : IRequest
{
    public long SessionId { get; init; }
    public string DateFrom { get; set; }
}

public record StatisticBalanceDateSavedEvent : INotification
{
    public long SessionId { get; init; }
    public int? LastSentMessageId { get; init; }
}

public class StatisticBalanceDateSavedHandler(IUserSessionService userSessionService, ICurrencyProvider currencyProvider, IMessageService messageService) : INotificationHandler<StatisticBalanceDateSavedEvent>
{
    public async Task Handle(StatisticBalanceDateSavedEvent notification, CancellationToken cancellationToken)
    {
        var message = await messageService.EditSentTextMessageAsync(
            new Message()
            {
                ChatId = notification.SessionId,
                Id = notification.LastSentMessageId,
                Text = "Enter the currency",
                Options = MessageOptions.FromList(currencyProvider.GetCurrencies().Select(c => c.Name).ToList())
            }, cancellationToken);

        var session = userSessionService.GetUserSession(notification.SessionId);
        if (session != null)
        {
            session.LastSentMessageId = message.Id;
        }
    }
}