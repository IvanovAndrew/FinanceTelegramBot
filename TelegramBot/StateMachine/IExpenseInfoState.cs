using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.StateMachine;

interface IExpenseInfoState
{
    Task<Message> Request(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken);
    bool AnswerIsRequired { get; }
    IExpenseInfoState Handle(string text, CancellationToken cancellationToken);
}