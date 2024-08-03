using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    class EnterTheDateState : IExpenseInfoState
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly ILogger _logger;
        private readonly bool _askCustomDate;
        private readonly ExpenseBuilder _expenseBuilder = new();
        private IExpenseInfoState _previousState;

        public bool UserAnswerIsRequired => true;
    
        public EnterTheDateState(IDateTimeService dateTimeService, ILogger logger, IExpenseInfoState previousState, bool askCustomDate = false)
        {
            _dateTimeService = dateTimeService;
            _askCustomDate = askCustomDate;
            _logger = logger;
            _previousState = previousState;
        }

        public async Task<IMessage> Request(ITelegramBot botClient, long chatId, CancellationToken cancellationToken)
        {
            var info = "Enter the date";

            if (_askCustomDate)
            {
                return await botClient.SendTextMessageAsync(chatId, $"{info}", cancellationToken: cancellationToken);
            }
        
            var keyboard = TelegramKeyboard.FromButtons(new[]
                {
                    // first row
                    new TelegramButton{Text = "Today", CallbackData = "today"},
                    new TelegramButton{Text = "Yesterday", CallbackData = "yesterday"},
                    new TelegramButton{Text = "Other", CallbackData = "Other"},
                });
        
            return await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: info,
                keyboard: keyboard,
                cancellationToken: cancellationToken);
        }

        public async Task HandleInternal(IMessage message, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                if (_dateTimeService.TryParse(message.Text, out var date))
                {
                    _expenseBuilder.Date = date;
                }
            });
        }

        public IExpenseInfoState MoveToPreviousState(IStateFactory stateFactory)
        {
            return _previousState;
        }

        public IExpenseInfoState ToNextState(IMessage message, IStateFactory stateFactory,
            CancellationToken cancellationToken)
        {
            if (_expenseBuilder.Date == null && message.Text.ToLowerInvariant() == "other")
            {
                return stateFactory.CreateEnterTheDateState(this, true);
            }
        
            if (_expenseBuilder.Date == null)
            {
                _logger.LogDebug($"{message.Text} isn't a date");
                return stateFactory.CreateErrorWithRetryState($"{message.Text} isn't a date.", this);
            }

            return stateFactory.CreateEnterTheCategoryState(_expenseBuilder);
        }
    }
}