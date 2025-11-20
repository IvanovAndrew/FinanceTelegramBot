namespace Application.Contracts;

public interface IFnsAPIService
{
    public Task<IReadOnlyCollection<RawOutcomeItem>> GetCheck(CheckRequisite checkRequisite);
}