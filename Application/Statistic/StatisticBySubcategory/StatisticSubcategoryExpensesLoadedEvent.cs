using Application.AddMoneyTransfer;
using Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Statistic.StatisticBySubcategory;

public record StatisticSubcategoryExpensesLoadedEvent : INotification
{
    public long SessionId { get; init; }
    public IReadOnlyList<Outcome> Outcomes { get; init; }
    public Category Category { get; init; }
    public SubCategory SubCategory { get; init; }
    public YearMonth MonthFrom { get; init; } 
}

public class StatisticSubcategoryExpensesLoadedEventHandler(IConversation conversation, ILogger<StatisticSubcategoryExpensesLoadedEventHandler> logger) : INotificationHandler<StatisticSubcategoryExpensesLoadedEvent>
{
    public async Task Handle(StatisticSubcategoryExpensesLoadedEvent notification, CancellationToken cancellationToken)
    {
        var outcomes = notification.Outcomes;
        if (outcomes.Any())
        {
            var currencies = outcomes.Select(c => c.Amount.Currency).Distinct().ToArray();
            
            var expenseAggregator = new ExpensesAggregator<string>(e => e.SubCategory!.Name, true, sortAsc: false);
            var aggregation = expenseAggregator.Aggregate(outcomes, currencies);

            var statistic = StatisticMapper.Map(aggregation, new StringColumnFactory());

            var table = StatisticTableBuilder.BuildTable(statistic, new TableOptions()
            {
                Subtitle = $"Category: {notification.Category.Name}{Environment.NewLine}Expenses from {notification.MonthFrom.ToString(DateFormat.FullMonthName)}",
                FirstColumnName = "Subcategory",
            });
            
            await conversation.Update(notification.SessionId, Screens.Notify(table), cancellationToken);
        }
    }
}