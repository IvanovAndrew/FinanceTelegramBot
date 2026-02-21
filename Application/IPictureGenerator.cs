using Domain;

namespace Application;

public interface IPictureGenerator
{
    byte[] GeneratePlot(IReadOnlyList<MonthlyBalance> data, Currency currency, PictureOptions options);
    byte[] GeneratePlot(IReadOnlyList<(YearMonth date, decimal value)> data, Currency currency, PictureOptions options);
}

public record PictureOptions(string Title, string xLable = "Date", string yLable = "Amount");