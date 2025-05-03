using MediatR;

namespace Application.AddMoneyTransfer;

public class AddMoneyTransferDescriptionCommandHandler(IMediator mediator, IUserSessionService userSessionService)
    : IRequestHandler<AddMoneyTransferDescriptionCommand>
{
    public async Task Handle(AddMoneyTransferDescriptionCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            session.MoneyTransferBuilder.Description = request.Description;
            session.QuestionnaireService.Next();

            await mediator.Publish(new OutcomeDescriptionAddedEvent()
                { SessionId = request.SessionId }, cancellationToken);
        }
    }
}