using Microsoft.AspNetCore.Mvc;
using StateMachine;
using Telegram.Bot.Types;
using TelegramBot.Services;

namespace TelegramBot.Controllers
{
    [ApiController]
    [Route(Route)]
    public class BotController : ControllerBase
    {
        private const string Route = "api";
        private const string MessageRoute = "message/update";
        private const string WebhookRoute = "webhook";
        private readonly ILogger _logger;
        private readonly TelegramBotService _botService;
        private readonly StateFactory _stateFactory;

        public BotController(ILogger<BotController> logger, StateFactory stateFactory, TelegramBotService botService)
        {
            _logger = logger;
            _stateFactory = stateFactory;
            _botService = botService;
        }

        [HttpPost]
        [Route(MessageRoute)]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            var botClient = new TelegramBotLogDecorator(_botService.GetBot(), _logger);

            var message = TelegramMessage.FromUpdate(update);
        
            var botEngine = new BotEngine(_stateFactory, _logger);

            try
            {
                await botEngine.Proceed(message, botClient);
            }
            catch (Exception e)
            {
                _logger.LogError($"Unhandled exception: {e}");
                if (message.ChatId != _botService.SupportChatId)
                {
                    await botClient.SendTextMessageAsync(message.ChatId, "Something went wrong. I've already informed Andrew about it.");
                    await botClient.SendTextMessageAsync(_botService.SupportChatId, $"Your wife is angry because of {e}");
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.ChatId, "Uncovered error {e}");
                }
            }
            
            return Ok();
        }

        [HttpPost]
        [Route(WebhookRoute)]
        public async Task<IActionResult> SetWebHook(string url)
        {
            var bot = _botService.GetBot();

            var hookUrl = url;

            var path = Route + "/" + MessageRoute;
            if (!hookUrl.EndsWith(path))
            {
                hookUrl = url.Last() == '/' ? url : url + "/";
                hookUrl += path;
            }
            
            await bot.SetWebhookAsync(hookUrl);

            return Ok();
        }
        
        [HttpGet]
        [Route(WebhookRoute)]
        public async Task<IActionResult> GetWebHook()
        {
            var bot = _botService.GetBot();

            var webHook = await bot.GetWebhookInfoAsync();

            return Ok(webHook);
        }
    }
}