using Application.Contracts.FNS;
using Application.Core;
using Domain;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Fns;

public class RussianCheckExpenseJsonParser(IFnsShopNameResolver shopNameResolver, IExternalCategoryMapper externalCategoryMapper, ILogger<RussianCheckExpenseJsonParser> logger) : IExpenseJsonParser
{
    public Currency Currency => Currency.RUR;

    public bool CanParse(string json)
    {
        return json.Contains("messageFiscalSign", StringComparison.InvariantCultureIgnoreCase);
    }

    public Check ParseCheck(string json)
    {
        FnsCheckInfo check = JsonSerializer.Deserialize<FnsCheckInfo>(json);

        return ParseCheck(check);
    }

    public Check ParseCheck(FnsCheckInfo check)
    {
        var date = DateOnly.FromDateTime(check.DateTime.Hour < 4? check.DateTime.AddDays(-1) : check.DateTime);
        
        var shop = shopNameResolver.Resolve(check);
        var newCodes = new HashSet<string>();
        
        var outcomes = new List<Outcome>();
        foreach (var item in check.Items)
        {
            var amount = item.Sum / 100;
            var description = item.Name;
            
            Category category = Categories.Outcome.DefaultCategory;
            SubCategory? subCategory = null;
            if (item.ProductCodeNew?.Gs1m?.Gtin is { } productCode)
            {
                var mappingResult = externalCategoryMapper.Map(new ExternalCategory{Source = "Gtin", RawName = productCode});
                if (mappingResult.Item1 != Categories.Outcome.DefaultCategory)
                {
                    category = mappingResult.Item1;
                    subCategory = mappingResult.Item2;
                    
                    logger.LogInformation($"Fount category for product code {productCode}: {category.Code}");
                }
                else
                {
                    newCodes.Add(productCode);
                    logger.LogInformation($"New product code: {productCode}");
                }
            }

            var expense = new Outcome()
            {
                Amount = new Money { Amount = amount, Currency = Currency },
                Shop = shop,
                Description = description,
                Date = date,
                Category = category,
                SubCategory = subCategory,
            };
            
            outcomes.Add(expense);
        }

        return new Check(){Outcomes = outcomes, NewOptions = newCodes};
    }
}