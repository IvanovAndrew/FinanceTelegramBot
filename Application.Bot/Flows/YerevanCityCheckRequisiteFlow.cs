using Application.Bot;
using Application.Bot.Flows;
using Application.Core.Services;
using MediatR;

namespace Application.Core;

public class YerevanCityCheckRequisiteFlow(IDateTimeService dateTimeService) : UserFlow
{
    private readonly YerevanCityCheckRequisiteFlowResolver _resolver = new();
    
    internal YerevanCityCheckRequisiteDraft Draft { get; } = new();

    public YerevanCityCheckRequisite ToEntity() => Draft.ToEntity();
    
    public override void ComputeStep()
    {
        var localStep = _resolver.Resolve(Draft);
        CurrentStep = Map(localStep);
    }

    private FlowStep Map(GetYerevanCityCheckRequisiteFlowStep localStep)
    {
        return localStep switch
        {
            GetYerevanCityCheckRequisiteFlowStep.AskDate => FlowStep.AskDay,
            GetYerevanCityCheckRequisiteFlowStep.AskCustomDate => FlowStep.AskCustomDay,
            GetYerevanCityCheckRequisiteFlowStep.AskCheckId => FlowStep.AskOrderId,
            GetYerevanCityCheckRequisiteFlowStep.DownloadCheck => FlowStep.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(localStep), localStep, null)
        };
    }

    public override Task HandleInput(FlowStep step, string text, CancellationToken ct)
    {
        switch (step)
        {
            case FlowStep.AskDay:
            case FlowStep.AskCustomDay:
                if (dateTimeService.TryParseDate(text, out var date))
                {
                    Draft.SetDate(date);
                }
                else if (text == "Another day")
                {
                    Draft.SetCustomDate();
                }
                break;

            case FlowStep.AskOrderId:
                Draft.SetCheckId(text);
                break;
        }
        
        return Task.CompletedTask;
    }

    public override Task Complete(long sessionId, IMediator mediator, CancellationToken ct) =>
        mediator.Send(new DownloadExpensesFromYerevanCityServiceCommand { SessionId = sessionId, CheckRequisite = ToEntity() }, ct);

    internal override ExtraData GetExtraData()
    {
        return new ExtraData();
    }
}