using GoogleSheetWriter;
using GoogleSheetWriter.Domain;

namespace GoogleSpreadsheet;

public static class Mapper
{
    public static MoneyTransferDto ToDto(MoneyTransfer moneyTransfer)
    {
        return new MoneyTransferDto()
        {
            Date = moneyTransfer.Date,
            Category = moneyTransfer.Category,
            Subcategory = moneyTransfer.Subcategory,
            Shop = moneyTransfer.Shop,
            Description = moneyTransfer.Description,
            Amount = moneyTransfer.Amount,
            Currency = moneyTransfer.Currency
        };
    }

    public static CurrencyExchangeDto ToCurrencyExchangeDto(CurrencyExchange currencyExchange)
    {
        return new CurrencyExchangeDto()
        {
            Date = currencyExchange.Date,
            Shop = currencyExchange.Shop,
            Description = currencyExchange.Description,
            SourceAmount = currencyExchange.SourceAmount,
            SourceCurrency = currencyExchange.SourceCurrency,
            TargetAmount = currencyExchange.TargetAmount,
            TargetCurrency = currencyExchange.TargetCurrency
        };
    }

    public static FutureExpenseDto ToFutureExpenseDto(FutureExpense futureExpense)
    {
        return new FutureExpenseDto()
        {
            Name = futureExpense.Name,
            Category = futureExpense.Category,
            Subcategory = futureExpense.Subcategory,
            Shop = futureExpense.Shop,
            Frequency = futureExpense.Frequency,
            Way = futureExpense.Way,
            Amount = futureExpense.Amount,
            Currency = futureExpense.Currency
        };
    }
}