using Domain;

namespace Infrastructure.GoogleSpreadsheet;

[Serializable]
public class GoogleSpreadsheetExpenseDto
{
    public DateTime Date { get; set; }
    public string Category { get; set; }
    public string? Subcategory { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }

    public static GoogleSpreadsheetExpenseDto FromExpense(IMoneyTransfer expense)
    {
        return new GoogleSpreadsheetExpenseDto()
        {
            Date = expense.Date.ToDateTime(default),
            Category = expense.Category.Name,
            Subcategory = expense.SubCategory?.Name,
            Description = expense.Description,
            Amount = expense.Amount.Amount,
            Currency = expense.Amount.Currency.Name
        };
    }

    public static IMoneyTransfer ToExpense(GoogleSpreadsheetExpenseDto dto, ICategoryProvider categoryProvider)
    {
        Domain.Currency currency = int.Parse(dto.Currency) switch
        {
            0 => Domain.Currency.Rur,
            1 => Domain.Currency.Amd,
            2 => Domain.Currency.Gel,
            3 => Domain.Currency.USD,
            4 => Domain.Currency.EUR,
            _ => throw new ArgumentOutOfRangeException($"Unknown currency code {dto.Currency}")
        };

        var domainCategory = categoryProvider.GetCategoryByName(dto.Category, false);
        var category = domainCategory ?? Domain.Category.FromString(dto.Category);
        
        var domainSubcategory = category.GetSubcategoryByName(dto.Subcategory);

        return new Outcome()
        {
            Date = DateOnly.FromDateTime(dto.Date),
            Category = category,
            SubCategory = domainSubcategory?? SubCategory.FromString(dto.Subcategory),
            Description = dto.Description,
            Amount = new Money()
            {
                Amount = dto.Amount,
                Currency = currency
            }
        };
    }
}