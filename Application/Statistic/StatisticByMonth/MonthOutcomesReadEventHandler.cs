using Application.AddMoneyTransfer;
using Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Statistic.StatisticByMonth;

public class MonthOutcomesReadEventHandler(IMessageService messageService, ILogger<MonthOutcomesReadEventHandler> logger) : INotificationHandler<MonthOutcomesReadEvent>
{
    public async Task Handle(MonthOutcomesReadEvent notification, CancellationToken cancellationToken)
    {
        if (!notification.Outcomes.Any())
            return;
        
        var expenseAggregator = new ExpensesAggregator<string>(e => e.Category?.Name?? "Unknown", true, sortAsc: false);

        var tableOptions = new TableOptions()
        {
            Subtitle = $"Expenses for {notification.Month.ToString(DateFormat.FullMonthName)}",
            FirstColumnName = "Category",
        };
        
        var currencies = notification.Outcomes.Select(c => c.Amount.Currency).Distinct().ToArray();
        var statistic = expenseAggregator.Aggregate(notification.Outcomes, currencies);
        var wrapper = StatisticMapper.Map(statistic, new StringColumnFactory());

        var table = StatisticTableBuilder.BuildTable(wrapper, tableOptions);
        
        await messageService.EditSentTextMessageAsync(
            new Message()
            {
                ChatId = notification.SessionId,
                Id = notification.LastSentMessageId,
                Table = table
            }, cancellationToken
        );
    }
}