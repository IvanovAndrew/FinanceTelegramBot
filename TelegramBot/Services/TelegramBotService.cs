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

        internal long SupportChatId => _supportChatId;

        public TelegramBotService(string token, long supportChatId)
        {
            _token = token;
            _supportChatId = supportChatId;
        }

        public ITelegramBot GetBot()
        {
            if (_botClient != null) return _wrappedBot!;

            _botClient = new TelegramBotClient(_token) {Timeout = TimeSpan.FromSeconds(15)};

            _wrappedBot = new TelegramBotClientImpl(_botClient); 
            return _wrappedBot;
        }
    }
}