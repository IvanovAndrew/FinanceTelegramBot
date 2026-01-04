using Domain;

namespace Application;

public interface IPictureGenerator
{
    byte[] GeneratePlot(IReadOnlyList<MonthlyBalance> data, Currency currency);
}