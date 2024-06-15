using System.Collections.Concurrent;
using Infrastructure;
using Microsoft.Extensions.Logging;

namespace StateMachine
{
    public class BotEngine
    {
        private readonly StateFactory _stateFactory;
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
            _logger.LogInformation($"{message.Text} was received");

            var allCommands = TelegramCommand.GetAllCommands(); 
            
            await botClient.SetMyCommandsAsync(
                allCommands
                    .OrderBy(c => c.Order)
                    .Select(c => new TelegramButton() { Text = c.Text, CallbackData = c.Command })
                    .ToArray()
                );

            bool stateExists = _answers.TryGetValue(message.ChatId, out var state);
            IExpenseInfoState newState;

            if (stateExists)
            {
                await RemovePreviousMessage(state!, botClient, message.ChatId);

                if (TelegramCommand.TryGetCommand(message.Text, out var command))
                {
                    newState = await command.Execute(state, _stateFactory);
                    _answers[message.ChatId] = newState;
                    var response = await newState.Request(botClient, message.ChatId);
                    _sentMessage[newState] = response;

                    return response;
                }
                
                if (message.Edited)
                {
                    state = state.PreviousState;
                }
                
                await state.Handle(message, default);
                _answers[message.ChatId] = newState = state.ToNextState(message, default);
            }
            else
            {
                newState = _stateFactory.CreateGreetingState();
            }

            if (newState is ILongTermOperation longOperation)
            {
                _sentMessage[state!] = await longOperation.Handle(botClient, message, default);
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
                    _answers[message.ChatId] = newState = newState.ToNextState(message, default);

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

        private async Task RemovePreviousMessage(IExpenseInfoState state, ITelegramBot botClient, long chatId, CancellationToken cancellationToken = default)
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
                        await botClient.DeleteMessageAsync(chatId, previousMessage.Id, cancellationToken);
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