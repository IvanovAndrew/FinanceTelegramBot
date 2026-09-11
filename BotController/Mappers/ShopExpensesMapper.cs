using Domain;
using TelegramBot.Contracts;

namespace TelegramBot.Mappers;

public static class ShopExpensesMapper
{
    public static ShopExpensesDto ToDto(ShopExpenses shopExpenses)
    {
        return new ShopExpensesDto()
        {
            Date = shopExpenses.Date,
            Shop = shopExpenses.Shop.Name,
            Total = shopExpenses.Total.Amount,
            Currency = shopExpenses.Currency.Name,
            
            Expenses = shopExpenses.Outcomes.Select(MoneyTransferMapper.ToDto).ToList()
        };
    }
}