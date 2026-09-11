using Application.Core;
using Domain;

namespace Application.Test.Stubs;

public class YerevanCityReceiptProviderStub : IYerevanCityReceiptProvider
{
    internal Dictionary<(DateOnly, string), IReadOnlyList<Outcome>> Responses = new();
    
    public Task<Check> ProcessAsync(DateOnly orderDate, string orderId, CancellationToken cancellationToken)
    {
        return Task.FromResult(new Check(){ Outcomes = Responses[(orderDate, orderId)], NewOptions = []});
    }
}