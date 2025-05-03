using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;

namespace TelegramBot.Controllers;

[ApiController]
public class TelegramBotOptionController : ControllerBase
{
    private readonly ITelegramBotClient _telegramBotClient;
    private const string WebhookRoute = "webhook";

    public TelegramBotOptionController(ITelegramBotClient telegramBotClient)
    {
        _telegramBotClient = telegramBotClient;
    }
    
    [HttpPost]
    [Route(WebhookRoute)]
    public async Task<IActionResult> SetWebHook(string url)
    {
        var hookUrl = url;

        var path = BotController.Route + "/" + BotController.MessageRoute;
        if (!hookUrl.EndsWith(path))
        {
            hookUrl = url.Last() == '/' ? url : url + "/";
            hookUrl += path;
        }
            
        await _telegramBotClient.SetWebhook(hookUrl);

        return Ok();
    }
        
    [HttpGet]
    [Route(WebhookRoute)]
    public async Task<IActionResult> GetWebHook()
    {
        var webHook = await _telegramBotClient.GetWebhookInfo();

        return Ok(webHook);
    }
}