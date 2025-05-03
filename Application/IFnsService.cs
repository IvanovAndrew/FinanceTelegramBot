using Domain;

namespace Application;

public interface IFnsService
{
    public Task<IReadOnlyCollection<Outcome>> GetCheck(string qrRaw);
}

public class FiscalData
{
    public string DocumentNumber { get; set; }
    public string DocumentSign { get; set; }
}