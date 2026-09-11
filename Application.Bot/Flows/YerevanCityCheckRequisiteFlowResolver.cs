using Application.Bot;

namespace Application.Core;

public enum GetYerevanCityCheckRequisiteFlowStep
{
    AskDate,
    AskCustomDate,
    AskCheckId,
    DownloadCheck,
}

internal class YerevanCityCheckRequisiteFlowResolver
{
    public GetYerevanCityCheckRequisiteFlowStep Resolve(YerevanCityCheckRequisiteDraft draft)
    {
        if (draft.Date == default && draft.IsCustomDate == default)
            return GetYerevanCityCheckRequisiteFlowStep.AskDate;

        if (draft.Date == default)
            return GetYerevanCityCheckRequisiteFlowStep.AskCustomDate;

        if (draft.CheckId == default)
            return GetYerevanCityCheckRequisiteFlowStep.AskCheckId;

        return GetYerevanCityCheckRequisiteFlowStep.DownloadCheck;
    }
}