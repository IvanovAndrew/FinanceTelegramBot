using Domain;
using TelegramBot.Contracts;

namespace TelegramBot.Mappers;

public static class MoneyTransferMapper
{
    public static MoneyTransferDTO ToDto(Outcome outcome)
    {
        return new MoneyTransferDTO
        {
            IsOutcome = true,
            Date = outcome.Date,
            Category = outcome.Category?.Code,
            SubCategory = outcome.SubCategory?.Code,
            Shop = outcome.Shop?.Name,
            Description = outcome.Description,
            Amount = outcome.Amount.Amount,
            Currency = outcome.Amount.Currency.Name
        };
    }
    
    public static MoneyTransferDTO ToDto(Income outcome)
    {
        return new MoneyTransferDTO
        {
            IsOutcome = false,
            Date = outcome.Date,
            Category = outcome.Category?.Code,
            SubCategory = outcome.SubCategory?.Code,
            Description = outcome.Description,
            Amount = outcome.Amount.Amount,
            Currency = outcome.Amount.Currency.Name
        };
    }
}