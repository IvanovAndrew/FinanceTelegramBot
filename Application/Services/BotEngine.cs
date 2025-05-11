using Application.AddExpense;
using Application.AddMoneyTransfer;
using Application.AddMoneyTransferByRequisites;
using Application.Commands;
using Application.Commands.CreateIncome;
using Application.Commands.SaveExpense;
using Application.Commands.StatisticByDay;
using Application.Commands.StatisticByMonth;
using Application.Commands.StatisticBySubcategory;
using Application.Commands.StatisticBySubcategoryByMonth;
using Application.Events;
using Application.Statistic.StatisticBalance;
using Application.Statistic.StatisticByCategory;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class BotEngine(IMediator mediator, IUserSessionService userSessionService, ILogger logger)
    {
        private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task Proceed(IMessage message, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{message.Text} was received");

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

            var session = userSessionService.GetUserSession(message.ChatId);

            if (session == null)
            {
                await mediator.Send(new StartSessionCommand() { SessionId = message.ChatId }, cancellationToken);
                return;
            }

            if (session.QuestionnaireService != null)
            {
                command = session.QuestionnaireService.GetHandler(session.Id, message.Text);
            }

            _logger.LogInformation($"Command is {command?.ToString()}. Text is {message.Text}");
            await mediator.Send(command, cancellationToken);
        }

        private IRequest? MapCommand(string text, long chatId)
        {
            IRequest? command = text switch
            {
                "/start" => new StartSessionCommand() { SessionId = chatId },
                "/cancel" => new CancelSessionCommand() { SessionId = chatId },
                "/back" => new StepBackCommand() { SessionId = chatId },
                "/save" => new SaveMoneyTransferCommand() { SessionId = chatId },
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
                "/requisites" => new RequisitesRequestedDomainEvent() { SessionId = chatId },
                "/check" => new CheckOutcomeQuestionnaireRequestedEvent() { SessionId = chatId },
                "/json" => new JsonCheckRequestedEvent() { SessionId = chatId },
                "/url" => new UrlLinkRequestedEvent() { SessionId = chatId },
                _ => null
            };
        }
    }
}