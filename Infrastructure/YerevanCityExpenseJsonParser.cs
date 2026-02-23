using System.Text.Json;
using Application;
using Application.Contracts;
using Domain;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Infrastructure;

public class YerevanCityExpenseJsonParser(IExternalCategoryMapper externalCategoryMapper) : IExpenseJsonParser
{
    private const string Prefix = "Ереван-сити";
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };
    
    public Currency Currency => Currency.AMD;
    public bool CanParse(string json)
    {
        return json.Contains("yerevan-city", StringComparison.OrdinalIgnoreCase);
    }

    public IReadOnlyList<Outcome> ParseOutcomes(string json, Category defaultCategory)
    {
        var orderResponse = JsonSerializer.Deserialize<YerevanCityOrderResponse>(json, JsonSerializerOptions);

        if (orderResponse?.Data?.OrderItems == null || !orderResponse.Success)
        {
            return [];
        }

        var date = DateOnly.FromDateTime(orderResponse.Data.CreateDate);

        return orderResponse.Data.OrderItems.Select(orderItem => MapToOutcome(orderItem, date, defaultCategory)).ToList();
    }

    private Outcome MapToOutcome(OrderItem item, DateOnly date, Category defaultCategory)
    {
        var amount = item.Price;

        var (category, subcategory) = externalCategoryMapper.Map(new ExternalCategory()
            { Source = Shops.YerevanCity, RawName = item.CategoryName }, defaultCategory);

        return new Outcome()
        {
            Amount = new Money { Amount = amount, Currency = Currency },
            Description = $"{Prefix}: {item.Name}",
            Date = date,
            Category = category,
            SubCategory = subcategory,
        };
    }
}