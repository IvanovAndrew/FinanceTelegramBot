﻿using Domain;
using MediatR;

namespace Application.Statistic.StatisticBalance;

public class StatisticBalanceSaveDateCommand : IRequest
{
    public long SessionId { get; init; }
    public string DateFrom { get; set; }
}

public class StatisticBalanceDateSaved : INotification
{
    public long SessionId { get; init; }
    public int? LastSentMessageId { get; init; }
}

public class StatisticBalanceDateSavedHandler(IUserSessionService userSessionService, IMessageService messageService) : INotificationHandler<StatisticBalanceDateSaved>
{
    public async Task Handle(StatisticBalanceDateSaved notification, CancellationToken cancellationToken)
    {
        var message = await messageService.EditSentTextMessageAsync(
            new Message()
            {
                ChatId = notification.SessionId,
                Id = notification.LastSentMessageId,
                Text = "Enter the currency",
                Options = MessageOptions.FromList(Currency.GetAvailableCurrencies().Select(c => c.Name).ToList())
            }, cancellationToken);

        var session = userSessionService.GetUserSession(notification.SessionId);
        if (session != null)
        {
            session.LastSentMessageId = message.Id;
        }
    }
}