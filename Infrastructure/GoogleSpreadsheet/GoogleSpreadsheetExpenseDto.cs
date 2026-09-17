using Application.Core.Services;
using Domain;

namespace Infrastructure.GoogleSpreadsheet;

[Serializable]
public class GoogleSpreadsheetExpenseDto
{
    public DateOnly Date { get; set; }
    public string Category { get; set; }
    public string? Subcategory { get; set; }
    public string? Shop { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }

    public static GoogleSpreadsheetExpenseDto FromExpense(Outcome expense)
    {
        return new GoogleSpreadsheetExpenseDto()
        {
            Date = expense.Date,
            Category = expense.Category.Name,
            Subcategory = expense.SubCategory?.Name,
            Shop = expense.Shop?.Name,
            Description = expense.Description,
            Amount = expense.Amount.Amount,
            Currency = expense.Amount.Currency.Name
        };
    }

    public static (Outcome? outcome, CategoryResolutionIssueBase? issue) ToExpense(GoogleSpreadsheetExpenseDto dto)
    {
        var currency = Domain.Currency.Parse(dto.Currency);

        var category = Categories.Outcome.GetCategory(dto.Category);
        if (category == null)
        {
            return (null, new CategoryResolutionIssue
                {
                    Date = dto.Date, 
                    RawCategory = dto.Category, 
                    Description = dto.Description
                });
        }
        
        var domainSubcategory = category.Sub(dto.Subcategory);
        if (category != null && category.Subcategories.Any() && domainSubcategory == null)
        {
            return (null, new SubCategoryResolutionIssue
                { 
                    Date = dto.Date, 
                    RawCategory = dto.Category, 
                    RawSubcategory = dto.Subcategory, 
                    Description = dto.Description 
                });
        }

        return (new Outcome()
        {
            Date = dto.Date,
            Category = category,    
            SubCategory = domainSubcategory,
            Shop = Domain.Shop.Create(dto.Shop),
            Description = dto.Description,
            
            Amount = new Money()
            {
                Amount = dto.Amount,
                Currency = currency
            }
        }, null);
    }
}