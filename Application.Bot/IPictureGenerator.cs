using Application.Core;
using Domain;

namespace Application.Bot;

public interface IPictureGenerator
{
    byte[] GeneratePlot(IReadOnlyList<MonthlyBalance> data, Currency currency, PictureOptions options);
    byte[] GeneratePlot(IReadOnlyList<(ChartBucket bucket, decimal value)> data, Currency currency, PictureOptions options);
}

public record PictureOptions(string Title, string xLable = "Date", string yLable = "Amount");