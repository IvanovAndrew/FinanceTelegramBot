using Domain;
using Domain.Services;
using Microsoft.Extensions.Logging;

namespace Infrastructure.GoogleSpreadsheet;

[Serializable]
public class GoogleSpreadsheetFutureExpenseDto
{
    public string Name { get; set; }
    public string Category { get; set; }
    public string? Subcategory { get; set; }
    public string? Shop { get; set; }
    public string Frequency { get; set; }
    public string Way { get; set; }
    public decimal? Amount { get; set; }
    public string Currency { get; set; }
    
    public static RecurringExpenseDefinition ToRecurringExpenseDefinition(GoogleSpreadsheetFutureExpenseDto dto, ILogger<IGoogleSpreadsheetService> logger)
    {
        var category = Categories.Outcome.GetCategory(dto.Category);
        if (category == null)
        {
            logger.LogError($"Category for {dto.Category} not found");
        }
        
        var domainSubcategory = category.Sub(dto.Subcategory);
        if (category != null && category.Subcategories.Any() && domainSubcategory == null)
        {
            logger.LogWarning($"Subcategory for {dto.Category} {dto.Subcategory} not found");
        }

        Money? expectedAmount = null;
        if (dto.Way == "Fixed" && dto.Amount != null)
        {
            var currency = Domain.Currency.Parse(dto.Currency);
            expectedAmount = new Money(){Amount = dto.Amount.Value, Currency = currency};
        }
        
        return new RecurringExpenseDefinition(
            dto.Name,
            category,
            domainSubcategory,
            Domain.Shop.Create(dto.Shop),
            Enum.Parse<RecurringFrequency>(dto.Frequency),
            Enum.Parse<Way>(dto.Way.Replace(" ", "")),
            expectedAmount
        );
    }
}