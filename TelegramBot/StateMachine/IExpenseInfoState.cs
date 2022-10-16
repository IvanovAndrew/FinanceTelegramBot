using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.StateMachine;

interface IExpenseInfoState
{
    Task<Message> Request(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken);
    IExpenseInfoState Handle(string text, CancellationToken cancellationToken);
}