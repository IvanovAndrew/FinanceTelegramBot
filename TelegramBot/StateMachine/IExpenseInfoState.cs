using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.StateMachine;

interface IExpenseInfoState
{
    bool UserAnswerIsRequired { get; }
    IExpenseInfoState PreviousState { get; }
    Task<Message> Request(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken);
    IExpenseInfoState Handle(string text, CancellationToken cancellationToken);
}