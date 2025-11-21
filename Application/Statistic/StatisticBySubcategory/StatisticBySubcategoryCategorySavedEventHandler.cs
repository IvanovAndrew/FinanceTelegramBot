using Application.Statistic.StatisticBySubcategory;
using MediatR;

namespace Application.Statistic;

public class StatisticBySubcategoryCategorySavedEventHandler(IUserSessionService userSessionService, IDateTimeService dateTimeService, IMessageService messageService) : INotificationHandler<StatisticBySubcategoryCategorySavedEvent>
{
    public async Task Handle(StatisticBySubcategoryCategorySavedEvent notification, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(notification.SessionId);

        if (session != null)
        {
            var today = dateTimeService.Today();
                
            await messageService.EditSentTextMessageAsync(
                new Message()
                {
                    ChatId = session.Id,
                    Id = session.LastSentMessageId,
                    Text = "Enter the date",
                    Options = MessageOptions.FromListAndLastSingleLine(
                        new []
                        {
                            today.ToString("MMMM yyyy"), 
                            today.AddMonths(-1).ToString("MMMM yyyy"), 
                            today.AddYears(-1).ToString("MMMM yyyy")
                        }, "Another month")
                },
                cancellationToken: cancellationToken);
        }
    }
}