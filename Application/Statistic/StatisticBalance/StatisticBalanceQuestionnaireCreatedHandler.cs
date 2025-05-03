using MediatR;

namespace Application.Statistic.StatisticBalance;

public class StatisticBalanceQuestionnaireCreatedHandler(IMessageService messageService, IDateTimeService dateTimeService) : INotificationHandler<StatisticBalanceQuestionnaireCreated>
{
    public async Task Handle(StatisticBalanceQuestionnaireCreated notification, CancellationToken cancellationToken)
    {
        var today = dateTimeService.Today();
        
        await messageService.EditSentTextMessageAsync(
            new Message()
            {
                ChatId = notification.SessionId,
                Id = notification.LastSentMessageId,
                Text = "Enter the month",
                Options = MessageOptions.FromListAndLastSingleLine(new []{today.ToString("MMMM yyyy"), today.AddMonths(-1).ToString("MMMM yyyy"), today.AddMonths(-6).ToString("MMMM yyyy")}, "Another month")
            }
        );
    }
}