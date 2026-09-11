using Application.Bot.Flows;
using Application.Core;
using Application.Core.AddMoneyTransfer;
using Application.Core.Commands;
using Application.Core.Events;
using Application.Core.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Bot
{
    public class BotEngine(
        IMediator mediator,
        ILogger<BotEngine> logger,
        IAuthenticationService authenticationService,
        IAdminNotificationService adminNotificationService)
    {
        private readonly CommandFactory _commandFactory = new();
        private readonly NotificationFactory _notificationFactory = new();

        public async Task Proceed(IIncomingMessage message, CancellationToken cancellationToken)
        {
            logger.LogInformation("Message received from ChatId: {ChatId}, Text: {Text}", message.ChatId, message.Text);

            if (!await authenticationService.IsUserAuthorized(message.ChatId, cancellationToken))
            {
                logger.LogWarning("Unauthorized access attempt from ChatId: {ChatId}", message.ChatId);
                await adminNotificationService.NotifyUnauthorizedAccessAsync(message.ChatId, cancellationToken);
                return;
            }

            if (string.IsNullOrWhiteSpace(message.Text))
            {
                if (message.FileInfo != null)
                {
                    var command = new DownloadJsonFileCommand { SessionId = message.ChatId, FileInfo = message.FileInfo };
                    await mediator.Send(command, cancellationToken);
                }
                return;
            }

            if (message.Text.StartsWith("/"))
            {
                if (await TryProcessCommand(message.Text, message.ChatId, cancellationToken))
                    return;

                if (await TryProcessNotification(message.Text, message.ChatId, cancellationToken))
                    return;
            }

            await mediator.Send(
                new UserInputReceivedCommand { SessionId = message.ChatId, Text = message.Text },
                cancellationToken);
        }

        private async Task<bool> TryProcessCommand(string commandText, long chatId, CancellationToken cancellationToken)
        {
            var command = _commandFactory.CreateCommand(commandText, chatId);
            if (command != null)
            {
                await mediator.Send(command, cancellationToken);
                return true;
            }
            return false;
        }

        private async Task<bool> TryProcessNotification(string commandText, long chatId, CancellationToken cancellationToken)
        {
            var notification = _notificationFactory.CreateNotification(commandText, chatId);
            if (notification != null)
            {
                await mediator.Publish(notification, cancellationToken);
                return true;
            }
            return false;
        }
    }

    internal class CommandFactory
    {
        public IRequest? CreateCommand(string commandText, long chatId)
        {
            return commandText switch
            {
                BotCommandConstants.Start => new StartSessionCommand { SessionId = chatId },
                BotCommandConstants.Cancel => new CancelSessionCommand { SessionId = chatId },
                BotCommandConstants.Back => new StepBackCommand { SessionId = chatId },
                BotCommandConstants.Outcome => new CreateExpenseCommand { SessionId = chatId },
                BotCommandConstants.Income => new CreateIncomeCommand { SessionId = chatId },
                BotCommandConstants.Balance => new SetStatisticsModeCommand { SessionId = chatId, Mode = StatisticsQueryMode.BalanceFromMonth },
                BotCommandConstants.DayByDay => new SetStatisticsModeCommand() { SessionId = chatId, Mode = StatisticsQueryMode.DayByDayExpenses },
                BotCommandConstants.StatisticByDay => new SetStatisticsModeCommand { SessionId = chatId, Mode = StatisticsQueryMode.DailyExpenses },
                BotCommandConstants.StatisticByMonth => new SetStatisticsModeCommand { SessionId = chatId, Mode = StatisticsQueryMode.MonthlyExpenses },
                BotCommandConstants.StatisticByCategory => new SetStatisticsModeCommand { SessionId = chatId, Mode = StatisticsQueryMode.CategoryByMonths },
                BotCommandConstants.StatisticBySubcategory => new SetStatisticsModeCommand { SessionId = chatId, Mode = StatisticsQueryMode.SubcategoryTotal },
                BotCommandConstants.StatisticBySubcategoryByMonth => new SetStatisticsModeCommand { SessionId = chatId, Mode = StatisticsQueryMode.SubcategoryByMonth },
                BotCommandConstants.Myself => new CreateOutcomeQuestionnaireCommand { SessionId = chatId },
                _ => null
            };
        }
    }

    internal class NotificationFactory
    {
        public INotification? CreateNotification(string commandText, long chatId)
        {
            return commandText switch
            {
                BotCommandConstants.Statistics => new StatisticRequestedEvent { SessionId = chatId },
                BotCommandConstants.Requisites => new RequisitesRequestedEvent { SessionId = chatId },
                BotCommandConstants.YerevanCity => new YerevanCityRequestedEvent { SessionId = chatId },
                BotCommandConstants.RussianShop => new RussianShopRequestedEvent { SessionId = chatId },
                BotCommandConstants.Check => new CheckOutcomeQuestionnaireRequestedEvent { SessionId = chatId },
                BotCommandConstants.Json => new JsonCheckRequestedEvent { SessionId = chatId },
                BotCommandConstants.Url => new UrlLinkRequestedEvent() { SessionId = chatId },
                _ => null
            };
        }
    }
}