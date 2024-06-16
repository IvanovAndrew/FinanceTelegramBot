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
    
        public IExpenseInfoState PreviousState { get; private set; }
        public bool UserAnswerIsRequired => true;
    
        public EnterTheDateState(IExpenseInfoState previousState, IDateTimeService dateTimeService, ILogger logger, bool askCustomDate = false)
        {
            _dateTimeService = dateTimeService;
            _askCustomDate = askCustomDate;
            _logger = logger;
            PreviousState = previousState;
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

        public async Task Handle(IMessage message, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                if (_dateTimeService.TryParse(message.Text, out var date))
                {
                    _expenseBuilder.Date = date;
                }
            });
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

            return stateFactory.CreateEnterTheCategoryState(_expenseBuilder, this);
        }
    }
}