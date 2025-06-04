using Domain;

namespace Infrastructure.GoogleSpreadsheet;

[Serializable]
public class GoogleSpreadsheetIncomeDto
{
    public DateTime Date { get; set; }
    public string Category { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }

    public static GoogleSpreadsheetIncomeDto FromIncome(IMoneyTransfer income)
    {
        return new GoogleSpreadsheetIncomeDto()
        {
            Date = income.Date.ToDateTime(default),
            Category = income.Category.Name,
            Description = income.Description,
            Amount = income.Amount.Amount,
            Currency = income.Amount.Currency.Name
        };
    }

    public static IMoneyTransfer ToIncome(GoogleSpreadsheetIncomeDto dto)
    {
        Domain.Currency currency;
        switch (int.Parse(dto.Currency))
        {
            case 0:
                currency = Domain.Currency.Rur;
                break;
            case 1:
                currency = Domain.Currency.Amd;
                break;
            case 2:
                currency = Domain.Currency.Gel;
                break;
            case 3:
                currency = Domain.Currency.USD;
                break;
            case 4:
                currency = Domain.Currency.EUR;
                break;
            default:
                throw new ArgumentOutOfRangeException($"Unknown currency code {dto.Currency}");
        }

        return new Income()
        {
            Date = DateOnly.FromDateTime(dto.Date),
            Category = Domain.Category.FromString(dto.Category),
            Description = dto.Description,
            Amount = new Money()
            {
                Amount = dto.Amount,
                Currency = currency
            }
        };
    }
}