using Application.Core;
using Application.Core.Statistic;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Bot.Statistic;

public class BalancePictureEventHandler(IPictureGenerator pictureGenerator, IConversation conversation, ILogger<BalancePictureEventHandler> logger)
    : INotificationHandler<BalanceStatisticCalculatedEvent>
{
    public async Task Handle(BalanceStatisticCalculatedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.MonthBalances.Count <= 2)
            return;
        
        var bytes = pictureGenerator.GeneratePlot(notification.MonthBalances, notification.Currency, new PictureOptions("Income / Outcome over time"));
        
        await conversation.Update(
            notification.SessionId, 
            Screens.Notify($"Balance for {notification.Currency} since {notification.MonthRange.From.ToString(DateFormat.FullMonthName)}", bytes), 
            cancellationToken);
    }
}