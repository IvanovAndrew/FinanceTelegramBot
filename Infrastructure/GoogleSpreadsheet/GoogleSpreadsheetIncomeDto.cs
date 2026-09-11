using Domain;

namespace Infrastructure.GoogleSpreadsheet;

[Serializable]
public class GoogleSpreadsheetIncomeDto
{
    public DateOnly Date { get; set; }
    public string Category { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }

    public static GoogleSpreadsheetIncomeDto FromIncome(IMoneyTransfer income)
    {
        return new GoogleSpreadsheetIncomeDto()
        {
            Date = income.Date,
            Category = income.Category.Name,
            Description = income.Description,
            Amount = income.Amount.Amount,
            Currency = income.Amount.Currency.Name
        };
    }

    public static Income ToIncome(GoogleSpreadsheetIncomeDto dto)
    {
        return new Income()
        {
            Date = dto.Date,
            Category = Categories.Income.GetCategory(dto.Category)?? Categories.Income.Others,
            Description = dto.Description,
            Amount = new Money()
            {
                Amount = dto.Amount,
                Currency = Domain.Currency.Parse(dto.Currency)
            }
        };
    }
}