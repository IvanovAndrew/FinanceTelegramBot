using Application.AddMoneyTransfer;
using Domain.Events;
using MediatR;

namespace Application.Events;

public class OutcomeSavedEventHandler : INotificationHandler<MoneyTransferSavedEvent>
{
    private readonly IUserSessionService _userSessionService;
    private readonly IMessageService _messageService;

    public OutcomeSavedEventHandler(IUserSessionService userSessionService, IMessageService messageService)
    {
        _userSessionService = userSessionService;
        _messageService = messageService;
    }
    
    public async Task Handle(MoneyTransferSavedEvent notification, CancellationToken cancellationToken)
    {
        var session = _userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            await _messageService.EditSentTextMessageAsync(new Message()
                { 
                    ChatId = notification.SessionId, 
                    Id = session.LastSentMessageId, 
                    Text = string.Join($"{Environment.NewLine}",
                        notification.MoneyTransfer.ToString(),
                        "", 
                        "Saved")
                }, cancellationToken);

            _userSessionService.RemoveSession(session.Id);
        }
    }
}