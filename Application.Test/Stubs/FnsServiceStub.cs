using Application.Contracts;

namespace Application.Test.Stubs;

public class FnsServiceStub : IFnsService
{
    internal Dictionary<CheckRequisite, IReadOnlyCollection<RawOutcomeItem>> Responses = new();
    
    public Task<IReadOnlyCollection<RawOutcomeItem>> GetCheck(CheckRequisite qrRaw)
    {
        if (Responses.TryGetValue(qrRaw, out var value))
        {
            return Task.FromResult(value);
        }

        value = Array.Empty<RawOutcomeItem>();
        
        return Task.FromResult(value);
    }
}