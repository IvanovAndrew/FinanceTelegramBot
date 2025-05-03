using Application.Events;
using Domain;
using MediatR;

namespace Application.AddMoneyTransfer;

public class AddExpensePriceCommandHandler : IRequestHandler<AddMoneyTransferPriceCommand>
{
    private readonly IMediator _mediator;
    private readonly IUserSessionService _userSessionService;

    public AddExpensePriceCommandHandler(IMediator mediator, IUserSessionService userSessionService)
    {
        _mediator = mediator;
        _userSessionService = userSessionService;
    }
    
    public async Task Handle(AddMoneyTransferPriceCommand request, CancellationToken cancellationToken)
    {
        var session = _userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            if (Money.TryParse(request.Price, out var price))
            {
                session.MoneyTransferBuilder.Sum = price;
                session.QuestionnaireService.Next();

                await _mediator.Publish(new OutcomePriceAddedEvent() { SessionId = session.Id }, cancellationToken);
            }
        }
    }
}