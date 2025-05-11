using Domain;

namespace Application;

public interface IFnsService
{
    public Task<IReadOnlyCollection<Outcome>> GetCheck(CheckRequisite checkRequisite);
}