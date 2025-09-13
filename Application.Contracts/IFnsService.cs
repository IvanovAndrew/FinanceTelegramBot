using Application.Contracts;

namespace Application;

public interface IFnsService
{
    public Task<IReadOnlyCollection<Outcome>> GetCheck(CheckRequisite checkRequisite);
}