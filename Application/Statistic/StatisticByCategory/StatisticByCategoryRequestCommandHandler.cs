using Application.AddMoneyTransfer;
using Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Statistic.StatisticByCategory;

public record GetStatisticCategoryRequestCommand : IRequest
{
    public long SessionId { get; init; }
    public StatisticsQuery Query { get; init; }
}

public class GetStatisticCategoryRequestCommandHandler(IFinanceRepository financeRepository, IMediator mediator) : IRequestHandler<GetStatisticCategoryRequestCommand>
{
    public async Task Handle(GetStatisticCategoryRequestCommand request, CancellationToken cancellationToken)
    {
        var query = request.Query;
    
        var filter = new FinanceFilter()
        {
            DateFrom = query.MonthRange.From.ToDateOnly(),
            Category = query.Category,
            Currency = query.Currency,
        };

        var outcomes = await financeRepository.ReadOutcomes(filter, cancellationToken);

        if (outcomes.Any())
        {
            await mediator.Publish(new CategoryOutcomesReadEvent()
                { 
                    SessionId = request.SessionId,
                    
                    Category = filter.Category,
                    MonthFrom = query.MonthRange.From,
                    Outcomes = outcomes
                }, 
                cancellationToken);
        }
    }
}

public record CategoryOutcomesReadEvent : INotification
{
    public long SessionId { get; init; }
    public Category Category { get; init; }
    
    public IReadOnlyList<Outcome> Outcomes { get; init; }
    public YearMonth MonthFrom { get; init; }
}

public class CategoryOutcomesReadTableEventHandler(IConversation conversation, ILogger<CategoryOutcomesReadTableEventHandler> logger) : INotificationHandler<CategoryOutcomesReadEvent>
{
    public async Task Handle(CategoryOutcomesReadEvent notification, CancellationToken cancellationToken)
    {
        if (!notification.Outcomes.Any())
            return;
        
        var expenseAggregator = new ExpensesAggregator<DateOnly>(e => e.Date.FirstDayOfMonth(), false, sortAsc: true);
        var currencies = notification.Outcomes.Select(c => c.Amount.Currency).Distinct().ToArray();
        var statistic = expenseAggregator.Aggregate(notification.Outcomes, currencies);
        
        var wrapper = StatisticMapper.Map(statistic, new DateOnlyColumnFactory());

        var tableOptions = new TableOptions()
        {
            Subtitle = $"Category: {notification.Category.Name}",
            FirstColumnName = "Month",
        };
        
        var table = StatisticTableBuilder.BuildTable(wrapper, tableOptions);
        
        await conversation.Update(notification.SessionId, Screens.Notify(table), cancellationToken);
    }
}

public class CategoryOutcomesReadDiagramEventHandler(IPictureGenerator pictureGenerator, IConversation conversation, ILogger<CategoryOutcomesReadTableEventHandler> logger) : INotificationHandler<CategoryOutcomesReadEvent>
{
    public async Task Handle(CategoryOutcomesReadEvent notification, CancellationToken cancellationToken)
    {
        if (!notification.Outcomes.Any())
            return;
        
        var expenseAggregator = new ExpensesAggregator<DateOnly>(e => e.Date.FirstDayOfMonth(), false, sortAsc: true);
        var currencies = notification.Outcomes.Select(c => c.Amount.Currency).Distinct().ToArray();
        var statistic = expenseAggregator.Aggregate(notification.Outcomes, currencies);

        foreach (var currency in currencies)
        {
            var data = statistic.Rows.Select(r => (YearMonth.From(r.Row), r[currency].Amount)).ToList();

            var title = $"{notification.Category.Name} expenses since {notification.Outcomes.Min(c => c.Date).ToString(DateFormat.FullMonthName)}";
            var bytes = pictureGenerator.GeneratePlot(data, currency, new PictureOptions(title));

            await conversation.Update(notification.SessionId, Screens.Notify(title, bytes), cancellationToken);
        }
    }
}