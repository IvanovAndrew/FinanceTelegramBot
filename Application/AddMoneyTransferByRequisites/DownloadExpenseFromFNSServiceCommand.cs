using Application.Commands.SaveOutcomesBatch;
using Application.Events;
using Domain;
using MediatR;

namespace Application.AddMoneyTransferByRequisites;

public class DownloadExpenseFromFNSServiceCommand : IRequest
{
    public long SessionId { get; init; }
    public CheckRequisite CheckRequisite { get; init; }
}

public class DownloadExpenseFromFNSServiceCommandHandler(IUserSessionService userSessionService, IFnsService fnsService, IMediator mediator) : IRequestHandler<DownloadExpenseFromFNSServiceCommand>
{
    public async Task Handle(DownloadExpenseFromFNSServiceCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            await mediator.Publish(new DownloadingExpenseStartedEvent(){SessionId = session.Id}, cancellationToken);
            
            var cancellationTokenSource = new CancellationTokenSource();
            session.CancellationTokenSource = cancellationTokenSource;

            IReadOnlyCollection<Outcome> outcomes = new List<Outcome>();
            
            using (cancellationTokenSource)
            {
                outcomes = await fnsService.GetCheck(request.CheckRequisite);
            }

            session.QuestionnaireService = null;
            
            await mediator.Publish(new DownloadingExpenseFinishedEvent(){SessionId = session.Id}, cancellationToken);

            await mediator.Send(new SaveOutcomesBatchCommand() { SessionId = session.Id, MoneyTransfers = outcomes }, cancellationToken);
        }
    }
}