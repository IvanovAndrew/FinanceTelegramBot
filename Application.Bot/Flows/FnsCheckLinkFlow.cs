using Application.Contracts;
using Application.Core;
using MediatR;

namespace Application.Bot.Flows;

public class FnsCheckLinkFlow() : UserFlow
{
    private string? _url;
    
    public override void ComputeStep()
    {
        CurrentStep = string.IsNullOrEmpty(_url)? FlowStep.AskUrlLink : FlowStep.Completed;
    }

    public override Task HandleInput(FlowStep step, string text, CancellationToken ct)
    {
        if (step == FlowStep.AskUrlLink)
        {
            _url = text;
        }
        
        return Task.CompletedTask;
    }

    public override Task Complete(long sessionId, IMediator mediator, CancellationToken ct) =>
        mediator.Send(new DownloadExpensesFromFNSServiceCommand { SessionId = sessionId, Today = DateOnlyUtils.Today, CheckRequisite = ToEntity() }, ct);

    public CheckRequisite ToEntity()
    {
        return CheckRequisite.FromUrlLink(_url);
    }

    internal override ExtraData GetExtraData()
    {
        return new ExtraData();
    }
}