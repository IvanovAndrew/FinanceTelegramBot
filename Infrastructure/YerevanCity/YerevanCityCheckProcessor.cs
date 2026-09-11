using Application.Contracts;
using Application.Core;

namespace Infrastructure.YerevanCity;

public class YerevanCityReceiptProvider(IYerevanCityAPI api, IExpenseJsonParser parser) : IYerevanCityReceiptProvider
{
    public async Task<Check> ProcessAsync(DateOnly orderDate, string orderId, CancellationToken cancellationToken)
    {
        string? json = await api.DownloadRawJson(orderDate, orderId, cancellationToken);

        if (string.IsNullOrWhiteSpace(json))
            throw new EmptyResponseException("yerevan-city");

        return parser.ParseCheck(json);
    }
}