using Domain.Events;
using Infrastructure;
using MediatR;

namespace Application.Events;

public class IncomeCreatedDomainEventHandler(IDateTimeService dateTimeService, IMessageService messageService)
    : INotificationHandler<IncomeCreatedEvent>
{
    public async Task Handle(IncomeCreatedEvent notification, CancellationToken cancellationToken)
    {
        await messageService.EditSentTextMessageAsync(
            new Message()
            {
                ChatId = notification.SessionId, 
                Id = notification.LastSentMessageId, 
                Text = "Enter the date",
                Options = MessageOptions.FromList(new []
                {
                    new Option(dateTimeService.Today().ToString("dd.MM.yyyy"), "Today"),
                    new Option(dateTimeService.Today().AddDays(-1).ToString("dd.MM.yyyy"), "Yesterday"),
                    new Option("Another day"),
                })
            },
            cancellationToken);
    }
}