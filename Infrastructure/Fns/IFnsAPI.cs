using Application.Contracts.FNS;
using Refit;

namespace Infrastructure.Fns;

public interface IFnsApi
{
    [Post("/api/v1/check/get")]
    Task<FnsResponse> GetCheck([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> parameters, CancellationToken cancellationToken);
}