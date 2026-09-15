using Application.Core;
using Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Bot.Statistic;

public record StatisticOutcomesReadEvent : INotification
{
    public long SessionId { get; init; }
    
    public StatisticWrapper Statistic { get; init; }
    
    public string Subtitle { get; init; }
    public string FirstColumnName { get; init; }
    public string? DiagramTitle { get; init; }

    public IReadOnlyList<(ChartBucket Bucket, IReadOnlyDictionary<Currency, Money> Values)> ChartPoints { get; init; } = [];
}

public class StatisticTableEventHandler(IConversation conversation) : INotificationHandler<StatisticOutcomesReadEvent>
{
    public async Task Handle(StatisticOutcomesReadEvent notification, CancellationToken cancellationToken)
    {
        if (notification.Statistic == null || notification.Statistic.Rows == null || !notification.Statistic.Rows.Any())
        {
            return;
        }
        
        var tableOptions = new TableOptions()
        {
            Subtitle = notification.Subtitle, 
            FirstColumnName = notification.FirstColumnName, 
        };
        
        var table = StatisticTableBuilder.BuildTable(notification.Statistic, tableOptions);

        await conversation.Update(notification.SessionId, Screens.Notify(table), cancellationToken);
    }
}

public class StatisticOutcomesDiagramEventHandler(IPictureGenerator pictureGenerator, IConversation conversation, ILogger<StatisticOutcomesDiagramEventHandler> logger) : INotificationHandler<StatisticOutcomesReadEvent>
{
    public async Task Handle(StatisticOutcomesReadEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation($"Handling {nameof(StatisticOutcomesReadEvent)}");
        
        if (!notification.ChartPoints.Any() || string.IsNullOrEmpty(notification.DiagramTitle))
            return;
        
        logger.LogInformation($"{notification.ChartPoints.Count} chart points");
        
        foreach (var currency in notification.ChartPoints.First().Values.Keys)
        {
            var data = notification.ChartPoints
                .Where(p => p.Values.ContainsKey(currency))
                .Select(p => (p.Bucket, p.Values[currency].Amount))
                .ToList();

            var bytes = pictureGenerator.GeneratePlot(data, currency, new PictureOptions(notification.DiagramTitle));
            await conversation.Update(notification.SessionId, Screens.Notify(notification.DiagramTitle, bytes), cancellationToken);
        }
        
        logger.LogInformation($"{nameof(StatisticOutcomesReadEvent)} finished");
    }
}