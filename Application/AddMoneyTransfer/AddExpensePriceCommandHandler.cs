using Application.Events;
using Domain;
using MediatR;

namespace Application.AddMoneyTransfer;

public class AddExpensePriceCommandHandler(IMediator mediator, IUserSessionService userSessionService)
    : IRequestHandler<AddMoneyTransferPriceCommand>
{
    public async Task Handle(AddMoneyTransferPriceCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            var parseResult = Money.Parse(request.Price);
            if (parseResult.IsSuccess)
            {
                session.MoneyTransferBuilder.Sum = parseResult.Value;
                session.QuestionnaireService.Next();

                await mediator.Publish(new OutcomePriceAddedEvent() { SessionId = session.Id }, cancellationToken);
            }
            else
            {
                await mediator.Publish(new WrongPriceEnteredEvent() { SessionId = session.Id, Error = parseResult.Error}, cancellationToken);
            }
        }
    }
}