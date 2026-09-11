using Application.Contracts;
using Application.Core;
using Domain;

namespace Application.Test.Stubs;

public class FnsReceiptProviderStub : IFnsReceiptProvider
{
    internal Dictionary<CheckRequisite, Check> Responses = new();
    
    public Task<Result<Check>> DownloadCheck(CheckRequisite requisite, CancellationToken cancellationToken)
    {
        if (!Responses.TryGetValue(requisite, out var check))
        {
            return Task.FromResult(Result<Check>.Failure("Receipt not found"));
        }
        
        return Task.FromResult(Result<Check>.Success(check));
    }
}