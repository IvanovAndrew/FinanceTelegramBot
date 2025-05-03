using Application.Commands.SaveExpense;
using MediatR;

namespace Application;

public class SavingExpenseNotificationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IMessageService _messageService;
    private readonly IUserSessionService _userSessionService;

    public SavingExpenseNotificationBehavior(IMessageService messageService, IUserSessionService userSessionService)
    {
        _messageService = messageService;
        _userSessionService = userSessionService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is SaveMoneyTransferCommand saveExpenseCommand)
        {
            var session = _userSessionService.GetUserSession(saveExpenseCommand.SessionId);
            if (session != null)
            {
                await _messageService.EditSentTextMessageAsync(new Message(){ChatId = saveExpenseCommand.SessionId, Id = session.LastSentMessageId, Text = "Saving..."}, cancellationToken: cancellationToken);
            }
        }

        return await next();
    }
}