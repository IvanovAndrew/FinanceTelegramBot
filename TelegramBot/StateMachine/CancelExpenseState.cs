using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBot.StateMachine
{
    class CancelExpenseState : IExpenseInfoState
    {
        public async Task<Message> Request(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
        {
            return await botClient.SendTextMessageAsync(chatId, "Operation is canceled", cancellationToken: cancellationToken);
        }

        public bool UserAnswerIsRequired => false;
        public IExpenseInfoState Handle(string text, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException();
        }
    }
}