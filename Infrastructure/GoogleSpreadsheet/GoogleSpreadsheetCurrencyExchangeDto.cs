using Domain;
using Microsoft.Extensions.Logging;

namespace Infrastructure.GoogleSpreadsheet;

[Serializable]
public class GoogleSpreadsheetCurrencyExchangeDto
{
    public DateOnly Date { get; set; }
    public string? Shop { get; set; }
    public string? Description { get; set; }
    
    public decimal SourceAmount { get; set; }
    public string SourceCurrency { get; set; }
    
    public decimal TargetAmount { get; set; }
    public string TargetCurrency { get; set; }

    public static CurrencyExchange ToCurrencyExchange(GoogleSpreadsheetCurrencyExchangeDto dto, ILogger logger)
    {
        var sourceCurrency = Currency.Parse(dto.SourceCurrency);
        var targetCurrency = Currency.Parse(dto.TargetCurrency);

        if (sourceCurrency == targetCurrency)
        {
            logger.LogWarning($"Source and target currency are the same ({sourceCurrency.Name})");
        }
        
        return new CurrencyExchange()
        {
            Date = dto.Date,
            Description = dto.Description,
            Shop = Domain.Shop.Create(dto.Shop),
            SourceAmount = new Money(){Amount = dto.SourceAmount, Currency = sourceCurrency},
            TargetAmount = new Money(){Amount = dto.TargetAmount, Currency = targetCurrency},
        };
    }
}