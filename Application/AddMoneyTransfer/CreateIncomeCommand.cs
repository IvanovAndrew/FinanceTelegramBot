using Domain.Events;
using MediatR;

namespace Application.Commands.CreateIncome;

public class CreateIncomeCommand : IRequest
{
    public long SessionID { get; init; }
}

public class CreateIncomeCommandHandler : IRequestHandler<CreateIncomeCommand>
{
    private readonly IUserSessionService _userSessionService;
    private readonly IMediator _mediator;

    public CreateIncomeCommandHandler(IUserSessionService userSessionService, IMediator mediator)
    {
        _userSessionService = userSessionService;
        _mediator = mediator;
    }
    
    public async Task Handle(CreateIncomeCommand request, CancellationToken cancellationToken)
    {
        var session = _userSessionService.GetUserSession(request.SessionID);

        if (session != null)
        {
            session.QuestionnaireService = new ManualMoneyTransferQuestionnaireService();
            session.MoneyTransferBuilder = new MoneyTransferBuilder(true);
            await _mediator.Publish(new IncomeCreatedEvent() { SessionId = session.Id, LastSentMessageId = (int) session.LastSentMessageId! }, cancellationToken);
        }
    }
}