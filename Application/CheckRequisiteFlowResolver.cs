namespace Application;

public enum GetCheckRequisiteFlowStep
{
    AskDateTime,
    AskSum,
    AskFiscalNumber,
    AskFiscalDocumentNumber,
    ASkFiscalDocumentSign,
    DownloadCheck,
}

internal class CheckRequisiteFlowResolver
{
    public GetCheckRequisiteFlowStep Resolve(CheckRequisiteDraft draft)
    {
        if (draft.DateTime == default)
            return GetCheckRequisiteFlowStep.AskDateTime;

        if (draft.Price == default)
            return GetCheckRequisiteFlowStep.AskSum;

        if (draft.FiscalNumber == default)
            return GetCheckRequisiteFlowStep.AskFiscalNumber;

        if (draft.FiscalDocumentNumber == default)
            return GetCheckRequisiteFlowStep.AskFiscalDocumentNumber;

        if (draft.FiscalDocumentSign == default)
            return GetCheckRequisiteFlowStep.ASkFiscalDocumentSign;

        return GetCheckRequisiteFlowStep.DownloadCheck;
    }
}