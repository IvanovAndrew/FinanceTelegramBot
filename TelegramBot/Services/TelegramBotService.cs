using Infrastructure;
using Telegram.Bot;

namespace TelegramBot.Services
{
    public class TelegramBotService
    {
        internal const string Route = "api/message/update";
        private TelegramBotClient? _botClient;
        private ITelegramBot? _wrappedBot;
        private readonly string _token;
        private readonly string _url;
        private readonly long _supportChatId;

        internal long SupportChatId => _supportChatId;

        public TelegramBotService(string url, string token, long supportChatId)
        {
            _url = url.Last() == '/' ? url : url + "/";
            _token = token;
            _supportChatId = supportChatId;
        }

        public async Task<ITelegramBot> GetBot()
        {
            if (_botClient != null) return _wrappedBot!;

            _botClient = new TelegramBotClient(_token) {Timeout = TimeSpan.FromSeconds(15)};

            var hook = $"{_url}{Route}";

            await _botClient.SetWebhookAsync(hook);

            _wrappedBot = new TelegramBotClientImpl(_botClient); 

            return _wrappedBot;
        }
    }
}