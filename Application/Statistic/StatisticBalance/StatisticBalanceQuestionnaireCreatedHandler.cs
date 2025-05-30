using MediatR;

namespace Application.Statistic.StatisticBalance;

public class StatisticBalanceQuestionnaireCreatedHandler(IMessageService messageService, IDateTimeService dateTimeService) : INotificationHandler<StatisticBalanceQuestionnaireCreated>
{
    public async Task Handle(StatisticBalanceQuestionnaireCreated notification, CancellationToken cancellationToken)
    {
        var today = dateTimeService.Today();
        
        var options = new List<DateOnly> { today, today.AddMonths(-1) };

        switch (today.Month)
        {
            case <= 2:
                options.Add(new(today.Year - 1, 1, 1));
                break;
            default:
                options.Add(new(today.Year, 1, 1));
                break;
        }
        
        await messageService.EditSentTextMessageAsync(
            new Message()
            {
                ChatId = notification.SessionId,
                Id = notification.LastSentMessageId,
                Text = "Enter the month",
                Options = MessageOptions.FromListAndLastSingleLine(options.Select(d => d.ToString("MMMM yyyy")).ToArray(), "Another month")
            }, cancellationToken);
    }
}