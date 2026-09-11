using Application.Core;
using Application.Core.Services;
using Domain;
using MediatR;

namespace Application.Bot.Flows;

public class FlowOrchestrator(IUserSessionService sessions, IFlowStepRenderer flowStepRenderer, IMediator mediator) : INotificationHandler<DraftUpdatedEvent>
{
    public async Task Handle(DraftUpdatedEvent e, CancellationToken ct)
    {
        var session = sessions.GetUserSession(e.SessionId);
        if (session?.ActiveFlow == null)
            throw new InvalidOperationException($"Session {e.SessionId} or its active flow not found");

        var flow = session.ActiveFlow;
        flow.ComputeStep();
        var step = flow.CurrentStep;

        if (flow.CurrentStep == FlowStep.Completed)
        {
            await flow.Complete(e.SessionId, mediator, ct);
            return;
        }
        
        var extraData = flow.GetExtraData();
        await flowStepRenderer.Render(e.SessionId, step, extraData, ct);
    }
}

public record ExtraData
{
    public Category? Category { get; init; }
    public bool ShowAllCategories { get; init; }
    public bool ShowAllCurrencies { get; init; }
    public string? ConfirmationText { get; set; }
}