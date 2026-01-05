using Application;
using Application.Services;
using Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using FileInfo = TelegramBot.Services.FileInfo;
using Message = Telegram.Bot.Types.Message;

namespace TelegramBot.Controllers
{
    [ApiController]
    [Route(Route)]
    public class BotController : ControllerBase
    {
        internal const string Route = "api";
        internal const string MessageRoute = "message/update";
        private readonly IMediator _mediator;
        private readonly IUserSessionService _userSessionService;
        private readonly ILogger _logger;

        public BotController(IMediator mediator, IUserSessionService userSessionService, ILogger<BotController> logger)
        {
            _mediator = mediator;
            _userSessionService = userSessionService;
            _logger = logger;
        }

        [HttpPost]
        [Route(MessageRoute)]
        public async Task<IActionResult> Post([FromBody] Update update, CancellationToken cancellationToken)
        {
            var message = FromUpdate(update);
        
            var botEngine = new BotEngine(_mediator, _userSessionService, _logger);

            try
            {
                await botEngine.Proceed(message, cancellationToken);
            }
            catch (BotException e)
            {
                // botEngine.ClearState(message.ChatId);
                _logger.LogError($"{e}");
                // await botClient.SendTextMessageAsync(new NotEditableMessageToSend(){ChatId = message.ChatId, Text = $"{e.Message}"});
            }
            catch (Exception e)
            {
                // botEngine.ClearState(message.ChatId);
                _logger.LogError($"Unhandled exception: {e}");
                // if (message.ChatId != _botService.SupportChatId)
                // {
                //     await botClient.SendTextMessageAsync(new NotEditableMessageToSend(){ ChatId = message.ChatId, Text = "Something went wrong. I've already informed Andrew about it."});
                //     await botClient.SendTextMessageAsync(new NotEditableMessageToSend(){ ChatId = _botService.SupportChatId, Text = $"Your wife is angry because of {e}"});
                // }
                // else
                // {
                //     await botClient.SendTextMessageAsync(new NotEditableMessageToSend(){ChatId = message.ChatId, Text = $"{e}"});
                // }
            }
            
            return Ok();

            Application.Message FromTelegramMessage(Message telegramMessage, string? text = null, bool edited = false)
            {
                return new Application.Message()
                {
                    Id = telegramMessage.MessageId,
                    ChatId = telegramMessage.Chat.Id,
                    Date = telegramMessage.Date,
                    Text = text?? telegramMessage.Text?? string.Empty,
                    Edited = edited,
                    FileInfo = telegramMessage.Document != null? new FileInfo()
                        { FileId = telegramMessage.Document.FileId, FileName = telegramMessage.Document.FileName, MimeType = telegramMessage.Document.MimeType } : null,
                };
            }
            
            Application.Message FromUpdate(Update update)
            {
                if (update.Type == UpdateType.Message)
                {
                    return FromTelegramMessage(update.Message!);
                }
                else if (update.Type == UpdateType.CallbackQuery)
                {
                    return FromTelegramMessage(update.CallbackQuery!.Message!, update.CallbackQuery.Data);
                }
                else if (update.Type == UpdateType.EditedMessage)
                {
                    return FromTelegramMessage(update.EditedMessage!, edited:true);
                }
        

                throw new ArgumentOutOfRangeException(
                    $"Unexpected type of Update. Expected {UpdateType.Message} or {UpdateType.CallbackQuery} but {update.Type} was received");
            }
        }
    }
}