using Application;
using Domain;

namespace UnitTest.Stubs;

public class FnsServiceStub : IFnsService
{
    internal Dictionary<CheckRequisite, IReadOnlyCollection<Outcome>> Responses = new();
    
    public Task<IReadOnlyCollection<Outcome>> GetCheck(CheckRequisite qrRaw)
    {
        if (Responses.TryGetValue(qrRaw, out var value))
        {
            return Task.FromResult(value);
        }

        value = Array.Empty<Outcome>();
        
        return Task.FromResult(value);
    }
}