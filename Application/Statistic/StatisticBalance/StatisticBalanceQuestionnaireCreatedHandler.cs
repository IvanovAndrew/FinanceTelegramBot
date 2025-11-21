using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Statistic.StatisticBalance;

public class StatisticBalanceQuestionnaireCreatedHandler(IMessageService messageService, IDateTimeService dateTimeService, ILogger<StatisticBalanceQuestionnaireCreatedHandler> logger) : INotificationHandler<StatisticBalanceQuestionnaireCreated>
{
    public async Task Handle(StatisticBalanceQuestionnaireCreated notification, CancellationToken cancellationToken)
    {
        logger.LogInformation($"{nameof(StatisticBalanceQuestionnaireCreated)} started {notification}");
        
        var today = dateTimeService.Today();
        
        var options = new List<DateOnly> { today, today.AddMonths(-1), today.AddMonths(-2) };

        logger.LogInformation($"{nameof(StatisticBalanceQuestionnaireCreated)}: options {string.Join(", ", options.Select(c => c))}");
        logger.LogInformation($"{nameof(StatisticBalanceQuestionnaireCreated)}: options {string.Join(", ", options.Select(c => c.ToString("MMMM yyyy")))}");
        
        await messageService.EditSentTextMessageAsync(
            new Message()
            {
                ChatId = notification.SessionId,
                Id = notification.LastSentMessageId,
                Text = "Enter the month",
                Options = MessageOptions.FromListAndLastSingleLine(options.Select(d => d.ToString("MMMM yyyy")).ToArray(), "Another month")
            }, cancellationToken);
        
        logger.LogInformation($"{nameof(StatisticBalanceQuestionnaireCreated)} finished");
    }
}