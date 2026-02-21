using Application.AddMoneyTransfer;
using Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Statistic.StatisticByCategory;

public record GetStatisticCategoryRequestCommand : IRequest
{
    public long SessionId { get; init; }
    public int LastSentMessageId { get; init; }
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
                    LastSentMessageId = request.LastSentMessageId,
                    
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
    public int LastSentMessageId { get; init; }
    public Category Category { get; init; }
    
    public IReadOnlyList<Outcome> Outcomes { get; init; }
    public YearMonth MonthFrom { get; init; }
}

public class CategoryOutcomesReadTableEventHandler(IMessageService messageService, ILogger<CategoryOutcomesReadTableEventHandler> logger) : INotificationHandler<CategoryOutcomesReadEvent>
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

public class CategoryOutcomesReadDiagramEventHandler(IPictureGenerator pictureGenerator, IMessageService messageService, ILogger<CategoryOutcomesReadTableEventHandler> logger) : INotificationHandler<CategoryOutcomesReadEvent>
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

            var bytes = pictureGenerator.GeneratePlot(data, currency, new PictureOptions($"{notification.Category.Name} expenses since {notification.Outcomes.Min(c => c.Date).ToString(DateFormat.FullMonthName)}"));

            await messageService.SendPictureAsync(
                new Message()
                {
                    ChatId = notification.SessionId,
                    PictureBytes = bytes,
                }, cancellationToken
            );
        }
    }
}