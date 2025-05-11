using Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.AddMoneyTransferByRequisites;

public class AddCheckDateCommand : IRequest
{
    public long SessionId { get; init; }
    public string Date { get; init; }
}

public class AddCheckDateCommandHandler(IUserSessionService userSessionService, IDateTimeService dateTimeService, IMediator mediator, ILogger<AddCheckDateCommandHandler> logger) : IRequestHandler<AddCheckDateCommand>
{
    public async Task Handle(AddCheckDateCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation($"{nameof(AddCheckDateCommandHandler)} called");
        
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            if (dateTimeService.TryParseDateTime(request.Date, out var dateTime))
            {
                session.CheckRequisites.DateTime = dateTime;
                session.QuestionnaireService.Next();

                await mediator.Publish(new CheckDateSavedEvent() { SessionId = session.Id }, cancellationToken);
            }
            else
            {
                await mediator.Publish(new CustomDateRequestedEvent()
                {
                    SessionId = session.Id,
                    LastSentMessageId = session.LastSentMessageId,
                    Text = $"Enter the day. Example: {dateTimeService.Today().ToString("dd MMMM yyyy")}",
                }, cancellationToken);
                session.LastSentMessageId = null;
            }
        }
    }
}

public class CheckDateSavedEvent : INotification
{
    public long SessionId { get; init; }
}

public class CheckDateSavedEventHandler(IMessageService messageService) : INotificationHandler<CheckDateSavedEvent>
{
    public async Task Handle(CheckDateSavedEvent notification, CancellationToken cancellationToken)
    {
        await messageService.SendTextMessageAsync(new Message()
        {
            ChatId = notification.SessionId,
            Text = "Enter the total price"
        }, cancellationToken);
    }
}