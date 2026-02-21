using MediatR;

namespace Application;

public record UserInputReceivedCommand : IRequest
{
    public long SessionId { get; init; }
    public int? LastSentMessageId { get; init; }
    public string Text { get; init; }
}

public class UserInputReceivedCommandHandler(IUserSessionService userSessionService, IMediator mediator) : IRequestHandler<UserInputReceivedCommand>
{
    private readonly IMediator _mediator;

    public async Task Handle(
        UserInputReceivedCommand cmd,
        CancellationToken ct)
    {
        var session = userSessionService.GetUserSession(cmd.SessionId);

        if (session == null)
            throw new InvalidOperationException("Session not found");

        var flow = session.ActiveFlow;

        if (flow == null)
            throw new InvalidOperationException("Active flow is missing");

        try
        {
            await flow.HandleInput(
                flow.CurrentStep,
                cmd.Text,
                ct);

            await mediator.Publish(
                new DraftUpdatedEvent { SessionId = session.Id },
                ct);
        }
        catch (FlowInputValidationException ex)
        {
            await mediator.Publish(
                new InvalidUserInputEvent
                {
                    SessionId = session.Id,
                    Step = ex.Step,
                    ErrorMessage = ex.Message
                },
                ct);
        }
        
    }
}


public record InvalidUserInputEvent : INotification
{
    public long SessionId { get; init; }
    public FlowStep Step { get; init; }
    public string ErrorMessage { get; init; } = "";
}

public class InvalidUserInputEventHandler(
    IMessageService messageService)
    : INotificationHandler<InvalidUserInputEvent>
{
    public async Task Handle(
        InvalidUserInputEvent e,
        CancellationToken ct)
    {
        await messageService.EditSentTextMessageAsync(
            new Message
            {
                ChatId = e.SessionId,
                Text = $"{e.ErrorMessage}"
            },
            ct);
    }
}
