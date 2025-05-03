using System.Text.RegularExpressions;
using MediatR;

namespace Application.AddMoneyTransferByRequisites;

public class AddCheckFiscalDocumentNumberCommand : IRequest
{
    public long SessionId { get; init; }
    public string DocumentNumber { get; init; }
}

public partial class AddCheckDocumentNumberCommandHandler(IUserSessionService userSessionService, IMediator mediator) : IRequestHandler<AddCheckFiscalDocumentNumberCommand>
{
    public async Task Handle(AddCheckFiscalDocumentNumberCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            if (DigitsOnlyRegex().IsMatch(request.DocumentNumber))
            {
                session.CheckRequisites.FiscalDocumentNumber = request.DocumentNumber;
                session.QuestionnaireService.Next();

                await mediator.Publish(new CheckFiscalDocumentNumberSaved() { SessionId = session.Id }, cancellationToken);
            }
        }
    }
    
    [GeneratedRegex(@"^\d+$")]
    private static partial Regex DigitsOnlyRegex();
}

public class CheckFiscalDocumentNumberSaved : INotification
{
    public long SessionId { get; init; }
}

public class CheckFiscalDocumentNumberSavedHandler(IUserSessionService userSessionService, IMessageService messageService) : INotificationHandler<CheckFiscalDocumentNumberSaved>
{
    public async Task Handle(CheckFiscalDocumentNumberSaved notification, CancellationToken cancellationToken)
    {
        await messageService.SendTextMessageAsync(new Message()
        {
            ChatId = notification.SessionId,
            Text = "Enter the fiscal document sign. It should contain only digits",
        }, cancellationToken);
    }
}