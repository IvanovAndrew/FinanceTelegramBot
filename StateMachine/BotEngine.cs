using System.Collections.Concurrent;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    public class BotEngine
    {
        private readonly IStateFactory _stateFactory;
        private static ConcurrentDictionary<long, IExpenseInfoState> _answers = new();
        private static ConcurrentDictionary<IExpenseInfoState, IMessage> _sentMessage = new();
        private readonly ILogger _logger;

        public BotEngine(IStateFactory stateFactory, ILogger logger)
        {
            _stateFactory = stateFactory?? throw new ArgumentNullException(nameof(stateFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IMessage> Proceed(IMessage message, ITelegramBot botClient)
        {
            _logger.LogInformation($"{message.Text} was received");

            var allCommands = TelegramCommand.GetAllCommands(); 
            
            await botClient.SetMyCommandsAsync(
                allCommands
                    .OrderBy(c => c.Order)
                    .Select(c => new TelegramButton() { Text = c.Text, CallbackData = c.Command })
                    .ToArray()
                );

            if (!_answers.TryGetValue(message.ChatId, out var state))
            {
                _answers[message.ChatId] = state = new InitialState();
            }
            
            IExpenseInfoState newState;

            await RemovePreviousMessage(state, botClient);

            if (message.Edited)
            {
                state = state.PreviousState;
            }
            
            await state.Handle(message, default);
            _answers[message.ChatId] = newState = state.MoveToNextState(message, _stateFactory, default);

            if (newState is ILongTermOperation longOperation)
            {
                _sentMessage[state] = await longOperation.Handle(botClient, message, default);
                while (!_answers.TryRemove(message.ChatId, out _)) { }

                return _sentMessage[state];
            }
            else
            {
                _answers[message.ChatId] = newState;

                _sentMessage[newState] =
                    await newState.Request(botClient, message.ChatId);
        
                while (!newState.UserAnswerIsRequired)
                {
                    await newState.Handle(message, default);
                    _answers[message.ChatId] = newState = newState.MoveToNextState(message, _stateFactory, default);

                    if (newState is ILongTermOperation longOperation1)
                    {
                        _sentMessage[state] = await longOperation1.Handle(botClient, message, default);
                        while (!_answers.TryRemove(message.ChatId, out _)) { }
                    }
                    else
                    {
                        var requestedMessage = await newState.Request(botClient, message.ChatId);
                        _sentMessage[newState] = requestedMessage;
                    }
                }
            
                return _sentMessage[newState];
            }
        }

        private async Task RemovePreviousMessage(IExpenseInfoState state, ITelegramBot botClient, CancellationToken cancellationToken = default)
        {
            if (_sentMessage.TryRemove(state, out var previousMessage))
            {
                try
                {
                    await botClient.DeleteMessageAsync(previousMessage, cancellationToken);
                }
                catch
                {
                    // ignore
                }
            }
        }
    }
}