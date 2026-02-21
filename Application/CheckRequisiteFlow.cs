using Application.Contracts;
using Domain.Check;

namespace Application;

public class CheckRequisiteFlow(IDateTimeService dateTimeService) : UserFlow
{
    private readonly CheckRequisiteFlowResolver _resolver = new();

    internal CheckRequisiteDraft Draft { get; } = new();

    public CheckRequisite ToEntity()
    {
        return Draft.ToEntity();
    }

    public override void ComputeStep()
    {
        var localStep = _resolver.Resolve(Draft);
        CurrentStep = Map(localStep);
    }
    
    private static FlowStep Map(GetCheckRequisiteFlowStep step) =>
        step switch
        {
            GetCheckRequisiteFlowStep.AskDateTime => FlowStep.AskDateTime,
            GetCheckRequisiteFlowStep.AskSum => FlowStep.AskAmount,
            GetCheckRequisiteFlowStep.AskFiscalNumber => FlowStep.AskFiscalNumber,
            GetCheckRequisiteFlowStep.AskFiscalDocumentNumber => FlowStep.AskFiscalDocumentNumber,
            GetCheckRequisiteFlowStep.ASkFiscalDocumentSign => FlowStep.AskFiscalDocumentSign,
            GetCheckRequisiteFlowStep.DownloadCheck => FlowStep.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(step))
        };

    public override Task HandleInput(FlowStep step, string text, CancellationToken ct)
    {
        switch (step)
        {
            case FlowStep.AskDay:
            case FlowStep.AskDateTime:
                if (dateTimeService.TryParseDateTime(text, out var date))
                {
                    Draft.SetDate(date);
                }
                break;
            
            case FlowStep.AskAmount:
                if (decimal.TryParse(text, out var value))
                {
                    Draft.SetPrice(value);
                }
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

    internal override ExtraData GetExtraData()
    {
        return new ExtraData();
    }
}