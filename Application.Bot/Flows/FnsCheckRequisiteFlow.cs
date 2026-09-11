using Application.Contracts;
using Application.Core;
using Application.Core.Services;
using Domain.Check;
using MediatR;

namespace Application.Bot.Flows;

public class FnsCheckRequisiteFlow(IDateTimeService dateTimeService) : UserFlow
{
    private readonly FnsCheckRequisiteFlowResolver _resolver = new();

    private FnsCheckRequisiteDraft Draft { get; } = new();

    public CheckRequisite ToEntity()
    {
        return Draft.ToEntity();
    }

    public override void ComputeStep()
    {
        var localStep = _resolver.Resolve(Draft);
        CurrentStep = Map(localStep);
    }
    
    private static FlowStep Map(GetFnsCheckRequisiteFlowStep step) =>
        step switch
        {
            GetFnsCheckRequisiteFlowStep.AskDateTime => FlowStep.AskDateTime,
            GetFnsCheckRequisiteFlowStep.AskSum => FlowStep.AskAmount,
            GetFnsCheckRequisiteFlowStep.AskFiscalNumber => FlowStep.AskFiscalNumber,
            GetFnsCheckRequisiteFlowStep.AskFiscalDocumentNumber => FlowStep.AskFiscalDocumentNumber,
            GetFnsCheckRequisiteFlowStep.ASkFiscalDocumentSign => FlowStep.AskFiscalDocumentSign,
            GetFnsCheckRequisiteFlowStep.DownloadCheck => FlowStep.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(step))
        };

    public override Task HandleInput(FlowStep step, string text, CancellationToken ct)
    {
        switch (step)
        {
            case FlowStep.AskDay:
            case FlowStep.AskDateTime:
                if (!dateTimeService.TryParseDateTime(text, out var date))
                    throw new FlowInputValidationException(step, "Could not recognize the date");
                Draft.SetDate(date);
                break;
            
            case FlowStep.AskAmount:
                if (!decimal.TryParse(text, out var value))
                    throw new FlowInputValidationException(step, "Could not recognize the amount");
                Draft.SetPrice(value);
                break;
            
            case FlowStep.AskFiscalNumber:
                
                var fiscalNumberResult = FiscalNumber.Create(text);
                if (!fiscalNumberResult.IsSuccess)
                {
                    throw new FlowInputValidationException(step, fiscalNumberResult.Error);
                }
                
                Draft.SetFiscalNumber(fiscalNumberResult.Value);
                
                break;
                
            case FlowStep.AskFiscalDocumentNumber:
                
                var fiscalDocumentNumberResult = FiscalDocumentNumber.Create(text);
                if (!fiscalDocumentNumberResult.IsSuccess)
                {
                    throw new FlowInputValidationException(step, fiscalDocumentNumberResult.Error);
                }
                
                Draft.SetDocumentNumber(fiscalDocumentNumberResult.Value);
                
                break;
                
            case FlowStep.AskFiscalDocumentSign:
                
                var fiscalDocumentSignResult = FiscalDocumentSign.Create(text);
                if (!fiscalDocumentSignResult.IsSuccess)
                {
                    throw new FlowInputValidationException(step, fiscalDocumentSignResult.Error);
                }
                
                Draft.SetDocumentSign(fiscalDocumentSignResult.Value);
                
                break;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(step), step, null);
        }
        
        return Task.CompletedTask;
    }

    public override Task Complete(long sessionId, IMediator mediator, CancellationToken ct) =>
        mediator.Send(new DownloadExpensesFromFNSServiceCommand { SessionId = sessionId, Today = DateOnlyUtils.Today, CheckRequisite = ToEntity() }, ct);

    internal override ExtraData GetExtraData()
    {
        return new ExtraData();
    }
}