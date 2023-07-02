using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    public class BotEngine
    {
        private readonly StateFactory _stateFactory;
        private readonly IDateTimeService _dateTimeService;
        private static ConcurrentDictionary<long, IExpenseInfoState> _answers = new();
        private static ConcurrentDictionary<IExpenseInfoState, IMessage> _sentMessage = new();
        private readonly ILogger _logger;

        public BotEngine(StateFactory stateFactory, ILogger logger)
        {
            _stateFactory = stateFactory?? throw new ArgumentNullException(nameof(stateFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IMessage> Proceed(IMessage message, ITelegramBot botClient)
        {
            await botClient.SetMyCommandsAsync(
                new[]
                {
                    new TelegramButton() { Text = "Start", CallbackData = "/start" },
                    new TelegramButton() { Text = "Back", CallbackData = "/back" },
                    new TelegramButton() { Text = "Cancel", CallbackData = "/cancel" },
                });

            _logger.LogInformation($"{message.Text} was received");
            if (!_answers.TryGetValue(message.ChatId, out var state))
            {
                _answers[message.ChatId] = state = _stateFactory.CreateGreetingState();
            }
            
            IExpenseInfoState newState;

            if (TryGetCommand(message.Text, botClient, _stateFactory, message.ChatId, out var command))
            {
                newState = await command.Execute(state);
                _answers[message.ChatId] = newState;
                var response = await newState.Request(botClient, message.ChatId);
                _sentMessage[newState] = response;

                return response;
            }
            
            await RemovePreviousMessage(state, botClient, message.ChatId);

            if (message.Edited)
            {
                state = state.PreviousState;
            }

            newState = state.Handle(message.Text, default);

            _answers[message.ChatId] = newState;

            _sentMessage[newState] =
                await newState.Request(botClient, message.ChatId);
            
            if (!newState.UserAnswerIsRequired)
            {
                _answers[message.ChatId] = newState = _stateFactory.CreateGreetingState();
                var requestedMessage = await newState.Request(botClient, message.ChatId);
                _sentMessage[newState] = requestedMessage;
                
                return requestedMessage;
            }
            else
            {
                return _sentMessage[newState];
            }
        }

        private bool TryGetCommand(string text, ITelegramBot telegramBot, StateFactory stateFactory, long chatId, [NotNullWhen(true)] out TelegramCommand? command)
        {
            if (text == "/start")
            {
                command = new StartCommand(telegramBot, stateFactory, chatId);
                return true;
            }
            
            if (text == "/back")
            {
                command = new BackCommand(telegramBot, stateFactory, chatId);
                return true;
            }
            
            if (text == "/cancel")
            {
                command = new CancelCommand(telegramBot, stateFactory, chatId);
                return true;
            }
            
            command = null;
            return false;
        }

        private async Task RemovePreviousMessage(IExpenseInfoState state, ITelegramBot botClient, long chatId, CancellationTokenSource cancellationTokenSource = default)
        {
            if (_sentMessage.TryRemove(state, out var previousMessage))
            {
                var diff = DateTime.Now.Subtract(previousMessage.Date);
                if (diff.Hours > 24)
                {
                    _logger.LogWarning(
                        $"Couldn't delete message {previousMessage.Id} {previousMessage.Text} because it was sent less than 24 hours ago");
                }
                else
                {
                    _logger.LogInformation($"Removing message {previousMessage.Id} \"{previousMessage.Text}\"");
                    try
                    {
                        await botClient.DeleteMessageAsync(chatId, previousMessage.Id, cancellationTokenSource.Token);
                        _logger.LogInformation($"Message {previousMessage.Id} \"{previousMessage.Text}\" is removed");
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"Couldn't delete message {previousMessage.Id} {previousMessage.Text}.", e);
                    }
                }
            }
        }
    }
}