using System.Text.Json;
using Application;
using Application.Contracts;
using Domain;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Infrastructure;

public class YerevanCityExpenseJsonParser(IExternalCategoryMapper externalCategoryMapper) : IExpenseJsonParser
{
    private const string Prefix = "Ереван-сити: ";
    private JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
    
    public Currency Currency => Currency.AMD;
    public bool CanParse(string json)
    {
        return json.Contains("Armenia") || json.Contains("Yerevan") || json.Contains("yerevan-city");
    }

    public IReadOnlyList<Outcome> ParseOutcomes(string json, Category defaultCategory)
    {
        var result = new List<Outcome>();
        
        var orderResponse = JsonSerializer.Deserialize<YerevanCityOrderResponse>(json, JsonSerializerOptions);

        if (orderResponse is not { Success: true })
            return [];

        var date = DateOnly.FromDateTime(orderResponse.Data.CreateDate);

        foreach (var orderItem in orderResponse.Data.OrderItems)
        {
            var amount = orderItem.Price;
            var description = orderItem.Name;

            var (category, subcategory) = externalCategoryMapper.Map(new ExternalCategory()
                { Source = Shops.YerevanCity, RawName = orderItem.CategoryName }, defaultCategory);

            var expense = new Outcome()
            {
                Amount = new Money { Amount = amount, Currency = Currency },
                Description = $"{Prefix}{description}",
                Date = date,
                Category = category,
                SubCategory = subcategory,
            };
            
            result.Add(expense);
        }

        return result;
    }
}