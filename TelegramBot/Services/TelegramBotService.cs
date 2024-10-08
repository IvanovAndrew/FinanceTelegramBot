using Infrastructure;
using Infrastructure.Telegram;
using StateMachine;
using Telegram.Bot;

namespace TelegramBot.Services
{
    public class TelegramBotService
    {
        private TelegramBotClient? _botClient;
        private ITelegramBot? _wrappedBot;
        private readonly string _token;
        private readonly long _supportChatId;
        private readonly IDateTimeService _dateTimeService;

        internal long SupportChatId => _supportChatId;

        public TelegramBotService(string token, long supportChatId, IDateTimeService dateTimeService)
        {
            _token = !string.IsNullOrEmpty(token)? token : throw new WrongConfigurationBotException(nameof(token));
            _supportChatId = supportChatId > 0? supportChatId : throw new WrongConfigurationBotException(nameof(supportChatId));
            _dateTimeService = dateTimeService;
        }

        public ITelegramBot GetBot()
        {
            if (_botClient != null) return _wrappedBot!;

            _botClient = new TelegramBotClient(_token) {Timeout = TimeSpan.FromSeconds(30)};

            _wrappedBot = new TelegramBotClientImpl(_botClient, _dateTimeService); 
            return _wrappedBot;
        }
    }
}