using Domain.Events;
using MediatR;

namespace Application.AddMoneyTransfer;

public class AddMoneyTransferDateCommandHandler : IRequestHandler<AddMoneyTransferDateCommand>
{
    private readonly IMediator _mediator;
    private readonly IUserSessionService _userSessionService;
    private readonly IDateTimeService _dateTimeService;

    public AddMoneyTransferDateCommandHandler(IMediator mediator, IUserSessionService userSessionService,
        IDateTimeService dateTimeService)
    {
        _mediator = mediator;
        _userSessionService = userSessionService;
        _dateTimeService = dateTimeService;
    }

    public async Task Handle(AddMoneyTransferDateCommand request, CancellationToken cancellationToken)
    {
        var session = _userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            var builder = session.MoneyTransferBuilder;

            if (_dateTimeService.TryParseDate(request.DateText, out var date))
            {
                builder.Date = date;
                session.QuestionnaireService.Next();
                await _mediator.Publish(new MoneyTransferDateEnteredEvent() { SessionId = request.SessionId },
                    cancellationToken);
            }
            else
            {
                await _mediator.Publish(new CustomDateChosenEvent() { SessionId = request.SessionId },
                    cancellationToken);
            }
        }
    }
}