using Application.Contracts;
using Domain;

namespace Application.Core;

public interface IFnsReceiptProvider
{
    Task<Result<Check>> DownloadCheck(CheckRequisite requisite, CancellationToken cancellationToken);
}