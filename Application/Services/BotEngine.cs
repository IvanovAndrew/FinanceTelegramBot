using Application.AddExpense;
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

            IRequest command = null;

            if (message.Text == "/start")
            {
                command = new StartSessionCommand() { SessionId = message.ChatId };
            }
            else if (message.Text == "/cancel")
            {
                command = new CancelSessionCommand() { SessionId = message.ChatId };
            }
            else if (message.Text == "/back")
            {
                command = new StepBackCommand() { SessionId = message.ChatId };
            }
            else if (message.Text == "/save")
            {
                command = new SaveMoneyTransferCommand() { SessionId = message.ChatId };
            }
            else if (message.Text == "/outcome")
            {
                command = new CreateExpenseCommand() { SessionID = message.ChatId };
            }
            else if (message.Text == "/income")
            {
                command = new CreateIncomeCommand() { SessionID = message.ChatId };
            }
            else if (message.Text == "/statistics")
            {
                var systemEvent = new StatisticRequestedEvent { SessionId = message.ChatId };
                await mediator.Publish(systemEvent, cancellationToken);
                return;
            }
            else if (message.Text == "/balance")
            {
                command = new StatisticBalanceCommand { SessionId = message.ChatId };
            }
            else if (message.Text == "/statisticByDay")
            {
                command = new StatisticByDayCommand { SessionId = message.ChatId };
            }
            else if (message.Text == "/statisticByMonth")
            {
                command = new StatisticByMonthCommand { SessionId = message.ChatId };
            }
            else if (message.Text == "/statisticByCategory")
            {
                command = new StatisticByCategoryCommand { SessionId = message.ChatId };
            }
            else if (message.Text == "/statisticBySubcategory")
            {
                command = new StatisticBySubcategoryCommand { SessionId = message.ChatId };
            }
            else if (message.Text == "/statisticBySubcategoryByMonth")
            {
                command = new StatisticBySubcategoryByMonthCommand { SessionId = message.ChatId };
            }
            else if (message.Text == "/myself")
            {
                command = new CreateOutcomeQuestionnaireCommand() { SessionId = message.ChatId };
            }
            else if (message.Text == "/check")
            {
                var domainEvent = new CheckOutcomeQuestionnaireRequestedEvent() { SessionId = message.ChatId };
                await mediator.Publish(domainEvent, cancellationToken);
                return;
            }
            else if (message.Text == "/json")
            {
                var domainEvent = new JsonCheckRequestedEvent() { SessionId = message.ChatId };
                await mediator.Publish(domainEvent, cancellationToken);
                return;
            }
            else if (message.Text == "/url")
            {
                await mediator.Publish(new UrlLinkRequestedEvent() { SessionId = message.ChatId },
                    cancellationToken);
                return;
            }
            else if (message.Text == "/requisites")
            {
                await mediator.Publish(new RequisitesRequestedDomainEvent() { SessionId = message.ChatId },
                    cancellationToken);
                return;
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
    }
}