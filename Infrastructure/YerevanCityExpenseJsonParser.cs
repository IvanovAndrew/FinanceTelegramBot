using Application.Contracts;
using Application.Core;
using Domain;
using Microsoft.Extensions.Logging;

namespace Infrastructure;

public class YerevanCityExpenseJsonParser(IExternalCategoryMapper externalCategoryMapper, ILogger<YerevanCityExpenseJsonParser> logger) : IExpenseJsonParser
{
    private readonly Currency _currency = Currency.AMD;
    
    public bool CanParse(string json)
    {
        return json.Contains("yerevan-city", StringComparison.OrdinalIgnoreCase);
    }

    public Check ParseCheck(string json)
    {
        var sanitizedJson = json.TrimStart('\uFEFF');
        
        var orderResponse = JsonSerializer.Deserialize<YerevanCityOrderResponse>(sanitizedJson);

        if (orderResponse?.Data?.OrderItems == null || !orderResponse.Success)
        {
            return new Check(){Outcomes = [], NewOptions = []};
        }

        var date = DateOnly.FromDateTime(orderResponse.Data.CreateDate.Hour < 4? orderResponse.Data.CreateDate.AddDays(-1): orderResponse.Data.CreateDate);
        
        var outcomes = new List<Outcome>(orderResponse.Data.OrderItems.Count);
        
        var newOptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in orderResponse.Data.OrderItems)
        {
            var outcome = MapToOutcome(item, date);
            if (outcome.Category == Categories.Outcome.DefaultCategory)
            {
                newOptions.Add(item.CategoryName);
            }
            
            outcomes.Add(outcome);
        }

        return new Check(){Outcomes = outcomes, NewOptions = newOptions};
    }

    private Outcome MapToOutcome(OrderItem item, DateOnly date)
    {
        var amount = item.Price;

        var (category, subcategory) = externalCategoryMapper.Map(new ExternalCategory()
            { Source = Shops.YerevanCity, RawName = item.CategoryName });

        return new Outcome()
        {
            Amount = new Money { Amount = amount, Currency = _currency },
            Shop = Shop.Create(Shops.YerevanCity),
            Description = item.Name,
            Date = date,
            Category = category,
            SubCategory = subcategory,
        };
    }
}