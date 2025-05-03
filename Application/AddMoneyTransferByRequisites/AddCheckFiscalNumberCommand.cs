using System.Text.RegularExpressions;
using MediatR;

namespace Application.AddMoneyTransferByRequisites;

public class AddCheckFiscalNumberCommand : IRequest
{
    public long SessionId { get; init; }
    public string FiscalNumber { get; init; }
}

public class AddCheckFiscalNumberCommandHandler(IUserSessionService userSessionService, IMediator mediator) : IRequestHandler<AddCheckFiscalNumberCommand>
{
    public async Task Handle(AddCheckFiscalNumberCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);
        if (session != null)
        {
            var fiscalNumber = request.FiscalNumber;
            fiscalNumber = fiscalNumber.Replace(" ", "");

            if (Regex.IsMatch(fiscalNumber, "\\d{16}"))
            {
                session.CheckRequisites.FiscalNumber = fiscalNumber;
                session.QuestionnaireService.Next();

                await mediator.Publish(new CheckFiscalNumberSavedEvent() { SessionId = session.Id }, cancellationToken);
            }
            else
            {
                await mediator.Publish(new CheckFiscalNumberNotSavedEvent() { SessionId = session.Id }, cancellationToken);
            }
        }
    }
}

public class CheckFiscalNumberNotSavedEvent : INotification
{
    public long SessionId { get; init; }
}

public class CheckFiscalNumberNotSavedEventHandler(IMessageService messageService) : INotificationHandler<CheckFiscalNumberSavedEvent>
{
    public async Task Handle(CheckFiscalNumberSavedEvent notification, CancellationToken cancellationToken)
    {
        await messageService.SendTextMessageAsync(new Message()
        {
            ChatId = notification.SessionId,
            Text = "Enter the fiscal number. It should contain 16 digits"
        }, cancellationToken);
    }
}

public class CheckFiscalNumberSavedEvent : INotification
{
    public long SessionId { get; init; }
}

public class CheckFiscalNumberSavedEventHandler(IMessageService messageService) : INotificationHandler<CheckFiscalNumberSavedEvent>
{
    public async Task Handle(CheckFiscalNumberSavedEvent notification, CancellationToken cancellationToken)
    {
        await messageService.SendTextMessageAsync(new Message()
        {
            ChatId = notification.SessionId,
            Text = "Enter the document number"
        }, cancellationToken);
    }
}