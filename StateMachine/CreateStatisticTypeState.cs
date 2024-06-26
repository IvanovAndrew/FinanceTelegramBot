﻿using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    internal class CreateStatisticTypeState : IExpenseInfoState
    {
        private readonly ILogger _logger;
    
        public bool UserAnswerIsRequired => true;
    
        public CreateStatisticTypeState(ILogger logger)
        {
            _logger = logger;
        }
    
        public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            var info = "Choose kind of statistic";

            var keyboard = 
                TelegramKeyboard.FromButtons(new []
                {
                    new TelegramButton { Text = "For a day", CallbackData = "statisticByDay"},
                    new TelegramButton { Text = "For a month", CallbackData = "statisticByMonth"}, 
                    new TelegramButton { Text = "For a category", CallbackData = "statisticByCategory"} 
                });
        
            return await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: info,
                keyboard: keyboard,
                cancellationToken: cancellationToken);
        }

        public Task Handle(IMessage message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory)
        {
            return stateFactory.CreateGreetingState();
        }

        public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
            CancellationToken cancellationToken)
        {
            if (message.Text == "statisticByDay") return stateFactory.CreateCollectDayExpenseState();
            if (message.Text == "statisticByMonth") return stateFactory.CreateCollectMonthStatisticState();
            if (message.Text == "statisticByCategory") return stateFactory.CreateCategoryForStatisticState();

            throw new BotStateException(new []{"statisticByDay", "statisticByMonth", "statisticByCategory"}, message.Text);
        }
    }
}