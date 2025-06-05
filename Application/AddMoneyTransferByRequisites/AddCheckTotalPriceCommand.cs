using MediatR;

namespace Application.AddMoneyTransferByRequisites;

public class AddCheckTotalPriceCommand : IRequest
{
    public long SessionId { get; init; }
    public string Price { get; init; }
}

public class AddCheckTotalPriceCommandHandler(IUserSessionService userSessionService, IMediator mediator) : IRequestHandler<AddCheckTotalPriceCommand>
{
    public async Task Handle(AddCheckTotalPriceCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            if (decimal.TryParse(request.Price, out var money))
            {
                session.CheckRequisites.TotalPrice = money;
                session.QuestionnaireService.Next();

                await mediator.Publish(new AddCheckTotalPriceSavedEvent() { SessionId = request.SessionId }, cancellationToken);
            }
        }
    }
}

public class AddCheckTotalPriceSavedEvent : INotification
{
    public long SessionId { get; init; }
}

public class AddCheckTotalPriceSavedEventHandler(IMessageService messageService) : INotificationHandler<AddCheckTotalPriceSavedEvent>
{
    public async Task Handle(AddCheckTotalPriceSavedEvent notification, CancellationToken cancellationToken)
    {
        await messageService.SendTextMessageAsync(new Message()
        {
            ChatId = notification.SessionId,
            Text = "Enter the fiscal number. It should contain 16 digits"
        }, cancellationToken);
    }
}