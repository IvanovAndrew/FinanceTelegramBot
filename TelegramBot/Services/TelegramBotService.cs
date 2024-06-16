using Infrastructure;
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
            _token = token;
            _supportChatId = supportChatId;
            _dateTimeService = dateTimeService;
        }

        public ITelegramBot GetBot()
        {
            if (_botClient != null) return _wrappedBot!;

            _botClient = new TelegramBotClient(_token) {Timeout = TimeSpan.FromSeconds(15)};

            _wrappedBot = new TelegramBotClientImpl(_botClient, _dateTimeService); 
            return _wrappedBot;
        }
    }
}