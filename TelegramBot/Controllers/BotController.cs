using Microsoft.AspNetCore.Mvc;
using StateMachine;
using Telegram.Bot.Types;
using TelegramBot.Services;

namespace TelegramBot.Controllers
{
    [ApiController]
    [Route(TelegramBotService.Route)]
    public class BotController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly TelegramBotService _bot;
        private readonly StateFactory _stateFactory;

        public BotController(ILogger<BotController> logger, StateFactory stateFactory, TelegramBotService bot)
        {
            _logger = logger;
            _stateFactory = stateFactory;
            _bot = bot;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            var botClient = await _bot.GetBot();

            var message = TelegramMessage.FromUpdate(update);
        
            var botEngine = new BotEngine(_stateFactory, _logger);

            try
            {
                await botEngine.Proceed(message, botClient);
            }
            catch (Exception e)
            {
                _logger.LogError($"Unhandled exception: {e}");
                if (message.ChatId != _bot.SupportChatId)
                {
                    await botClient.SendTextMessageAsync(message.ChatId, "Something went wrong. I've already informed Andrew about it.");
                    await botClient.SendTextMessageAsync(_bot.SupportChatId, $"Your wife is angry because of {e}");
                }
            }
            
            return Ok();
        }
    }
}