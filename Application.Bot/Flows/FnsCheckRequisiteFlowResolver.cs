using Application.Bot;

namespace Application.Core;

public enum GetFnsCheckRequisiteFlowStep
{
    AskDateTime,
    AskSum,
    AskFiscalNumber,
    AskFiscalDocumentNumber,
    ASkFiscalDocumentSign,
    DownloadCheck,
}

internal class FnsCheckRequisiteFlowResolver
{
    public GetFnsCheckRequisiteFlowStep Resolve(FnsCheckRequisiteDraft draft)
    {
        if (draft.DateTime == default)
            return GetFnsCheckRequisiteFlowStep.AskDateTime;

        if (draft.Price == default)
            return GetFnsCheckRequisiteFlowStep.AskSum;

        if (draft.FiscalNumber == default)
            return GetFnsCheckRequisiteFlowStep.AskFiscalNumber;

        if (draft.FiscalDocumentNumber == default)
            return GetFnsCheckRequisiteFlowStep.AskFiscalDocumentNumber;

        if (draft.FiscalDocumentSign == default)
            return GetFnsCheckRequisiteFlowStep.ASkFiscalDocumentSign;

        return GetFnsCheckRequisiteFlowStep.DownloadCheck;
    }
}