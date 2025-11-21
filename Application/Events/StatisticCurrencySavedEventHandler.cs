using MediatR;

namespace Application.Events;

public class StatisticCurrencySavedEventHandler(IUserSessionService userSessionService, IMessageService messageService) : INotificationHandler<StatisticCurrencySavedEvent>
{
    public Task Handle(StatisticCurrencySavedEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            
        }
        
        return Task.CompletedTask;
    }
}