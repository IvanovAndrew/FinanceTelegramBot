using Domain;
using MediatR;

namespace Application.Events;

public record EnterTheDayEvent : INotification
{
    public long SessionId { get; init; }
    public int? LastSentMessageId { get; init; }
}

public class EnterTheDayEventHandler(
    IDateTimeService dateTimeService,
    IMessageService messageService)
    : INotificationHandler<EnterTheDayEvent>
{
    public async Task Handle(EnterTheDayEvent notification, CancellationToken cancellationToken)
    {
        await messageService.EditSentTextMessageAsync(
            new Message()
            {
                ChatId = notification.SessionId, 
                Id = notification.LastSentMessageId, 
                Text = "Enter the date",
                Options = MessageOptions.FromList([
                    new Option(dateTimeService.Today().ToString(DateFormat.DayOnlyNumbers), "Today"),
                    new Option(dateTimeService.Today().AddDays(-1).ToString(DateFormat.DayOnlyNumbers), "Yesterday"),
                    new Option("Another day")
                ])
            },
            cancellationToken);
    }
}

public record EnterTheCustomDayEvent : INotification
{
    public long SessionId { get; init; }
    public int? LastSentMessageId { get; init; }
}

public class EnterTheCustomDayEventHandler(
    IDateTimeService dateTimeService,
    IMessageService messageService)
    : INotificationHandler<EnterTheCustomDayEvent>
{
    public async Task Handle(EnterTheCustomDayEvent notification, CancellationToken cancellationToken)
    {
        await messageService.EditSentTextMessageAsync(
            new Message()
            {
                ChatId = notification.SessionId, 
                Id = notification.LastSentMessageId, 
                Text = $"Enter the date. Example {dateTimeService.Today().ToString(DateFormat.DayOnlyNumbers)})",
            },
            cancellationToken);
    }
}

public record EnterTheMonthEvent : INotification
{
    public long SessionId { get; init; }
    public int? LastSentMessageId { get; init; }
}

public class EnterTheMonthEventHandler(
    IDateTimeService dateTimeService,
    IMessageService messageService)
    : INotificationHandler<EnterTheMonthEvent>
{
    public async Task Handle(EnterTheMonthEvent notification, CancellationToken cancellationToken)
    {
        await messageService.EditSentTextMessageAsync(
            new Message()
            {
                ChatId = notification.SessionId, 
                Id = notification.LastSentMessageId, 
                Text = "Enter the month",
                Options = MessageOptions.FromList([
                    new Option(dateTimeService.Today().ToString(DateFormat.FullMonthName)),
                    new Option(dateTimeService.Today().AddMonths(-1).ToString(DateFormat.FullMonthName)),
                    new Option("Another month")
                ])
            },
            cancellationToken);
    }
}

public record EnterTheCustomMonthEvent : INotification
{
    public long SessionId { get; init; }
    public int? LastSentMessageId { get; init; }
}

public class EnterTheCustomMonthEventHandler(
    IDateTimeService dateTimeService,
    IMessageService messageService)
    : INotificationHandler<EnterTheCustomMonthEvent>
{
    public async Task Handle(EnterTheCustomMonthEvent notification, CancellationToken cancellationToken)
    {
        await messageService.EditSentTextMessageAsync(
            new Message()
            {
                ChatId = notification.SessionId, 
                Id = notification.LastSentMessageId, 
                Text = $"Enter the month. Example: {dateTimeService.Today().ToString(DateFormat.FullMonthName)}",
            },
            cancellationToken);
    }
}

public record EnterTheCurrencyEvent : INotification
{
    public long SessionId { get; init; }
    public int? LastSentMessageId { get; init; }
}

public class EnterTheCurrencyEventHandler(
    IMessageService messageService)
    : INotificationHandler<EnterTheCurrencyEvent>
{
    public async Task Handle(EnterTheCurrencyEvent notification, CancellationToken cancellationToken)
    {
        var options = Currency.GetAvailableCurrencies()
                .Select(c => new Option(c.Name)).ToList();
        options.Add(new Option("All"));
        
        await messageService.EditSentTextMessageAsync(
            new Message()
            {
                ChatId = notification.SessionId, 
                Id = notification.LastSentMessageId, 
                Text = "Enter the currency",
                Options = MessageOptions.FromList(options)
            },
            cancellationToken);
    }
}