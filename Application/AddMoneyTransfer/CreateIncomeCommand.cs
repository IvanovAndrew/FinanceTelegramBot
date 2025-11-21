using MediatR;

namespace Application.AddMoneyTransfer;

public record CreateIncomeCommand : IRequest
{
    public long SessionID { get; init; }
}

public class CreateIncomeCommandHandler(IUserSessionService userSessionService, IMediator mediator)
    : IRequestHandler<CreateIncomeCommand>
{
    public async Task Handle(CreateIncomeCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionID);

        if (session != null)
        {
            session.QuestionnaireService = new ManualMoneyTransferQuestionnaireService();
            session.MoneyTransferBuilder = new MoneyTransferBuilder(true);
            await mediator.Publish(new IncomeCreatedEvent() { SessionId = session.Id, LastSentMessageId = session.LastSentMessageId }, cancellationToken);
        }
    }
}