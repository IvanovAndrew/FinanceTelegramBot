using Infrastructure;
using Infrastructure.Telegram;
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
        private readonly IStateFactory _stateFactory;

        public BotController(ILogger<BotController> logger, IStateFactory stateFactory, TelegramBotService botService)
        {
            _logger = logger;
            _stateFactory = stateFactory;
            _botService = botService;
        }

        [HttpPost]
        [Route(MessageRoute)]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            var botClient = new TelegramBotEditMessageInsteadOfSendingDecorator(new TelegramBotLogDecorator(_botService.GetBot(), _logger));

            var message = TelegramMessage.FromUpdate(update);
        
            var botEngine = new BotEngine(_stateFactory, _logger);

            try
            {
                await botEngine.Proceed(message, botClient);
            }
            catch (BotException e)
            {
                botEngine.ClearState(message.ChatId);
                _logger.LogError($"{e}");
                await botClient.SendTextMessageAsync(new NotEditableMessageToSend(){ChatId = message.ChatId, Text = $"{e.Message}"});
            }
            catch (Exception e)
            {
                botEngine.ClearState(message.ChatId);
                _logger.LogError($"Unhandled exception: {e}");
                if (message.ChatId != _botService.SupportChatId)
                {
                    await botClient.SendTextMessageAsync(new NotEditableMessageToSend(){ ChatId = message.ChatId, Text = "Something went wrong. I've already informed Andrew about it."});
                    await botClient.SendTextMessageAsync(new NotEditableMessageToSend(){ ChatId = _botService.SupportChatId, Text = $"Your wife is angry because of {e}"});
                }
                else
                {
                    await botClient.SendTextMessageAsync(new NotEditableMessageToSend(){ChatId = message.ChatId, Text = $"{e}"});
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