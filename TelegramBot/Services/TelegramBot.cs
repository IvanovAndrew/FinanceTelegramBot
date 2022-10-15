using Telegram.Bot;

namespace TelegramBot.Services;

public class TelegramBot
{
    internal const string Route = "api/message/update";
    private TelegramBotClient? _botClient;
    private readonly string _token;
    private readonly string _url;

    public TelegramBot(string url, string token)
    {
        _url = url.Last() == '/' ? url : url + "/";
        _token = token;
    }

    public async Task<TelegramBotClient> GetBot()
    {
        if (_botClient != null) return _botClient;

        _botClient = new TelegramBotClient(_token);

        var hook = $"{_url}{Route}";

        await _botClient.SetWebhookAsync(hook);

        return _botClient;
    }
}