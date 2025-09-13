using System.Text.RegularExpressions;
using Application.Contracts;
using MediatR;

namespace Application.AddMoneyTransferByRequisites;

public class AddCheckFiscalDocumentSignCommand : IRequest
{
    public long SessionId { get; init; }
    public string DocumentSign { get; init; }
}

public partial class AddCheckDocumentSignCommandHandler(IUserSessionService userSessionService, IMediator mediator) : IRequestHandler<AddCheckFiscalDocumentSignCommand>
{
    public async Task Handle(AddCheckFiscalDocumentSignCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            if (DigitsOnlyRegex().IsMatch(request.DocumentSign))
            {
                session.CheckRequisites.FiscalDocumentSign = request.DocumentSign;
                session.QuestionnaireService.Next();

                await mediator.Publish(new CheckDocumentSignSavedEvent(){SessionId = request.SessionId, CheckRequisite = session.CheckRequisites}, cancellationToken);
            }
        }
    }

    [GeneratedRegex("^\\d*$")]
    private static partial Regex DigitsOnlyRegex();
}

public class CheckDocumentSignSavedEvent : INotification
{
    public long SessionId { get; init; }
    public CheckRequisite CheckRequisite { get; init; }
}

public class CheckDocumentSignSavedEventHandler(IUserSessionService userSessionService, IMediator mediator) : INotificationHandler<CheckDocumentSignSavedEvent>
{
    public async Task Handle(CheckDocumentSignSavedEvent notification, CancellationToken cancellationToken)
    {
        await mediator.Send(new DownloadExpenseFromFNSServiceCommand() { SessionId = notification.SessionId, CheckRequisite = notification.CheckRequisite}, cancellationToken);
    }
}