using Infrastructure.Fns.DataContract;
using Refit;

namespace Infrastructure.Fns;

public interface IFnsApi
{
    [Post("/api/v1/check/get")]
    Task<FnsResponse> GetCheck([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> parameters);
}