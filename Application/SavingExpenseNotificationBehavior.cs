using Application.AddMoneyTransfer;
using MediatR;

namespace Application;

public class SavingExpenseNotificationBehavior<TRequest, TResponse>(
    IUserSessionService userSessionService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is SaveMoneyTransferCommand saveExpenseCommand)
        {
            var session = userSessionService.GetUserSession(saveExpenseCommand.SessionId);
            // if (session != null)
            // {
            //     await messageService.EditSentTextMessageAsync(new Message(){ChatId = saveExpenseCommand.SessionId, Id = saveExpenseCommand.LastSentMessageId, Text = "Saving..."}, cancellationToken: cancellationToken);
            // }
        }

        return await next();
    }
}