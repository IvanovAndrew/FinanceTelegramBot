using Domain.Services;
using TelegramBot.Contracts;

namespace TelegramBot.Mappers;

public static class FutureExpenseMapper
{
    public static FutureExpenseDTO ToDto(MissingRecurringExpense expense)
    {
        return new FutureExpenseDTO()
        {
            Name = expense.Definition.Name,
            Category = expense.Definition.Category.Code,
            Subcategory = expense.Definition.SubCategory?.Code,
            Shop = expense.Definition.Shop?.Name,
            Currency = expense.ResolvedAmount?.Currency.Name,
            Amount = expense.ResolvedAmount?.Amount ?? 0
        };
    }
}