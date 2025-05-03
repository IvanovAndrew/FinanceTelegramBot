using Application.Events;
using MediatR;

namespace Application.AddExpense;

public class CreateOutcomeQuestionnaireCommand : IRequest
{
    public long SessionId { get; init; }
}

public class CreateOutcomeQuestionnaireCommandHandler(IUserSessionService userSessionService, IMediator mediator) : IRequestHandler<CreateOutcomeQuestionnaireCommand>
{
    public async Task Handle(CreateOutcomeQuestionnaireCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            session.QuestionnaireService = new ManualMoneyTransferQuestionnaireService();
            session.MoneyTransferBuilder = new MoneyTransferBuilder(false);

            await mediator.Publish(
                new OutcomeQuestionnaireChosenEvent()
                    { SessionId = session.Id, LastSentMessageId = (int) session.LastSentMessageId! }, cancellationToken);
        }
    }
}