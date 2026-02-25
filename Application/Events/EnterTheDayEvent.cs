using Domain;
using MediatR;

namespace Application.Events;

public record EnterTheDayEvent : INotification
{
    public long SessionId { get; init; }
}

public class EnterTheDayEventHandler(
    IDateTimeService dateTimeService,
    IConversation conversation)
    : INotificationHandler<EnterTheDayEvent>
{
    public async Task Handle(EnterTheDayEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionId, Screens.SelectDay(dateTimeService.Today()), cancellationToken);
    }
}

public record EnterTheCustomDayEvent : INotification
{
    public long SessionId { get; init; }
}

public class EnterTheCustomDayEventHandler(
    IDateTimeService dateTimeService,
    IConversation conversation)
    : INotificationHandler<EnterTheCustomDayEvent>
{
    public async Task Handle(EnterTheCustomDayEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionId,
            Screens.SelectCustomDay(dateTimeService.Today()),
            cancellationToken);
    }
}

public record EnterTheMonthEvent : INotification
{
    public long SessionId { get; init; }
}

public class EnterTheMonthEventHandler(
    IDateTimeService dateTimeService,
    IConversation conversation)
    : INotificationHandler<EnterTheMonthEvent>
{
    public async Task Handle(EnterTheMonthEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionId,
            Screens.SelectMonth(dateTimeService.CurrentMonth()), cancellationToken);
    }
}

public record EnterTheCustomMonthEvent : INotification
{
    public long SessionId { get; init; }
}

public class EnterTheCustomMonthEventHandler(
    IDateTimeService dateTimeService,
    IConversation conversation)
    : INotificationHandler<EnterTheCustomMonthEvent>
{
    public async Task Handle(EnterTheCustomMonthEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionId,
            Screens.SelectCustomMonth(dateTimeService.CurrentMonth()), 
            cancellationToken);
    }
}

public record EnterTheCurrencyEvent : INotification
{
    public long SessionId { get; init; }
}

public class EnterTheCurrencyEventHandler(IConversation conversation)
    : INotificationHandler<EnterTheCurrencyEvent>
{
    public async Task Handle(EnterTheCurrencyEvent notification, CancellationToken cancellationToken)
    {
        await conversation.Update(notification.SessionId,
                Screens.SelectCurrency(Currency.GetAvailableCurrencies()),
                cancellationToken);
    }
}