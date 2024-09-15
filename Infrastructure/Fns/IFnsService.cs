using Infrastructure.Fns.DataContract;

namespace Infrastructure.Fns;

public interface IFnsService
{
    public Task<FnsResponse?> GetCheck(string qrRaw);
}