using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Statistic.StatisticBalance;

public class BalancePictureEventHandler(IPictureGenerator pictureGenerator, IConversation messageService, ILogger<BalancePictureEventHandler> logger)
    : INotificationHandler<BalanceStatisticCalculatedEvent>
{
    public async Task Handle(BalanceStatisticCalculatedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.MonthBalances.Count <= 2)
            return;
        
        var bytes = pictureGenerator.GeneratePlot(notification.MonthBalances, notification.Currency, new PictureOptions("Income / Outcome over time"));
        
        await messageService.Update(
            notification.SessionId, 
            Screens.Notify($"Balance for {notification.Currency} since {notification.MonthRange.From.ToString(DateFormat.FullMonthName)}", bytes), 
            cancellationToken);
    }
}