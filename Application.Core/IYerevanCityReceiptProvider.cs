namespace Application.Core;

public interface IYerevanCityReceiptProvider
{
    Task<Check> ProcessAsync(DateOnly orderDate, string orderId, CancellationToken cancellationToken);
}