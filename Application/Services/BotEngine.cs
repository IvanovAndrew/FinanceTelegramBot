using Application.AddMoneyTransfer;
using Application.AddMoneyTransferByRequisites;
using Application.Commands;
using Application.Events;
using Application.Statistic.StatisticBalance;
using Application.Statistic.StatisticByCategory;
using Application.Statistic.StatisticByDay;
using Application.Statistic.StatisticByMonth;
using Application.Statistic.StatisticBySubcategory;
using Application.Statistic.StatisticBySubcategoryByMonth;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class BotEngine(IMediator mediator, ILogger<BotEngine> logger)
    {
        public async Task Proceed(IMessage message, CancellationToken cancellationToken)
        {
            logger.LogInformation($"{message.Text} was received");

            IRequest? command = null;

            if (message.Text.StartsWith("/"))
            {
                command = MapCommand(message.Text, message.ChatId);
                if (command == null)
                {
                    var appEvent = MapNotification(message.Text, message.ChatId);
                    if (appEvent != null)
                    {
                        await mediator.Publish(appEvent, cancellationToken);
                        return;
                    }
                }
            }
            else if (string.IsNullOrEmpty(message.Text) && message.FileInfo != null)
            {
                command = new DownloadJsonFileCommand() { SessionId = message.ChatId, FileInfo = message.FileInfo };
            }

            if (command != null)
            {
                await mediator.Send(command, cancellationToken);
                return;
            }
            
            await mediator.Send(new UserInputReceivedCommand(){SessionId = message.ChatId, LastSentMessageId = message.Id, Text = message.Text}, cancellationToken);

            // if (session == null)
            // {
            //     await mediator.Send(new StartSessionCommand() { SessionId = message.ChatId }, cancellationToken);
            //     return;
            // }
        }

        private IRequest? MapCommand(string text, long chatId)
        {
            IRequest? command = text switch
            {
                "/start" => new StartSessionCommand() { SessionId = chatId },
                "/cancel" => new CancelSessionCommand() { SessionId = chatId },
                "/back" => new StepBackCommand() { SessionId = chatId },
                //"/save" => new SaveMoneyTransferCommand() { SessionId = chatId },
                "/outcome" => new CreateExpenseCommand() { SessionID = chatId },
                "/income" => new CreateIncomeCommand() { SessionID = chatId },
                "/balance" => new StatisticBalanceCommand { SessionId = chatId },
                "/statisticByDay" => new StatisticByDayCommand { SessionId = chatId },
                "/statisticByMonth" => new StatisticByMonthCommand { SessionId = chatId },
                "/statisticByCategory" => new StatisticByCategoryCommand { SessionId = chatId },
                "/statisticBySubcategory" => new StatisticBySubcategoryCommand { SessionId = chatId },
                "/statisticBySubcategoryByMonth" => new StatisticBySubcategoryByMonthCommand { SessionId = chatId },
                "/myself" => new CreateOutcomeQuestionnaireCommand() { SessionId = chatId },
                _ => null
            };

            return command;
        }

        private INotification? MapNotification(string text, long chatId)
        {
            return text switch
            {
                "/statistics" => new StatisticRequestedEvent { SessionId = chatId },
                "/requisites" => new RequisitesRequestedEvent() { SessionId = chatId },
                "/check" => new CheckOutcomeQuestionnaireRequestedEvent() { SessionId = chatId },
                "/json" => new JsonCheckRequestedEvent() { SessionId = chatId },
                "/url" => new EnterUrlLinkEvent() { SessionId = chatId },
                _ => null
            };
        }
    }
}