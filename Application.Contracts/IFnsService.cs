namespace Application.Contracts;

public interface IFnsService
{
    public Task<IReadOnlyCollection<RawOutcomeItem>> GetCheck(CheckRequisite checkRequisite);
}