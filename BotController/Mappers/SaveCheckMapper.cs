using Application.Core;
using Domain;
using TelegramBot.Contracts;

namespace TelegramBot.Mappers;

public static class SaveCheckMapper
{
    public static SaveCheckDto ToDto(SaveResult<ShopExpenses> saveResult)
    {
        return new SaveCheckDto()
        {
            Success = saveResult.Success,
            Error = saveResult.ErrorMessage,
            ShopExpenses = saveResult.Data != null? ShopExpensesMapper.ToDto(saveResult.Data) : null,
        };
    }
}