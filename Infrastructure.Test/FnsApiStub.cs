using Application.Contracts.FNS;
using Infrastructure.Fns;

namespace Infrastructure.Test;

public class FnsApiStub : IFnsApi
{
    public FnsResponse? Response { get; set; }
    
    public Task<FnsResponse> GetCheck(Dictionary<string, string> parameters, CancellationToken cancellationToken)
    {
        return Task.FromResult(Response);
    }
}