using Infrastructure.Fns;
using Infrastructure.Fns.DataContract;

namespace UnitTest;

public class FnsServiceStub : IFnsService
{
    internal Dictionary<string, FnsResponse?> Responses = new();
    
    public Task<FnsResponse?> GetCheck(string qrRaw)
    {
        if (Responses.TryGetValue(qrRaw, out var value))
        {
            return Task.FromResult(value);
        }

        return Task.FromResult((FnsResponse?) null);
    }
}