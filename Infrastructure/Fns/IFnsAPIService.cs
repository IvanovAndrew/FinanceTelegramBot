using Application.Contracts;
using Application.Contracts.FNS;
using Domain;

namespace Infrastructure.Fns;

public interface IFnsAPIService
{
    public Task<SaveResult<FnsCheckInfo>> GetCheck(CheckRequisite checkRequisite, CancellationToken cancellationToken);
}