using Domain;

namespace Application.Test.Stubs;

public class PictureGeneratorStub : IPictureGenerator
{
    public byte[] GeneratePlot(IReadOnlyList<MonthlyBalance> data, Currency currency, PictureOptions options)
    {
        return [1];
    }

    public byte[] GeneratePlot(IReadOnlyList<(YearMonth date, decimal value)> data, Currency currency, PictureOptions options)
    {
        return [2];
    }
}