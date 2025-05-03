using Application.AddMoneyTransfer;
using Domain.Events;
using MediatR;

namespace Application.Events;

public class OutcomeCreatedDomainEventHandler : INotificationHandler<OutcomeCreatedEvent>
{
    private readonly IUserSessionService _userSessionService;
    private readonly IMessageService _messageService;

    public OutcomeCreatedDomainEventHandler(IUserSessionService userSessionService, IMessageService messageService)
    {
        _userSessionService = userSessionService;
        _messageService = messageService;
    }
    
    public async Task Handle(OutcomeCreatedEvent notification, CancellationToken cancellationToken)
    {
        var session = _userSessionService.GetUserSession(notification.SessionID);

        if (session != null)
        {
            await _messageService.EditSentTextMessageAsync(
                new Message()
                {
                    ChatId = session.Id,
                    Id = session.LastSentMessageId,
                    Text = "Enter the way",
                    Options = MessageOptions.FromList(new []{new Option("/myself", "By myself"), new Option("/check", "From check")})
                }, cancellationToken);
        }
    }
}