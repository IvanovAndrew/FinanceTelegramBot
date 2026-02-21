using Application.AddMoneyTransfer;
using Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Statistic.StatisticBySubcategoryByMonth;

public class StatisticSubcategoryMonthRequestCommandHandler(IFinanceRepository financeRepository, IMediator mediator) : IRequestHandler<StatisticSubcategoryMonthRequestCommand>
{
    public async Task Handle(StatisticSubcategoryMonthRequestCommand request, CancellationToken cancellationToken)
    {
        var query = request.Query;
        
        var filter = new FinanceFilter()
        {
            DateFrom = query.MonthRange.From.ToDateOnly(),
            DateTo = query.MonthRange.To.ToDateOnly(),
            Category = query.Category,
            Subcategory = query.SubCategory,
            Currency = query.Currency,
        };
        
        var outcomes = await financeRepository.ReadOutcomes(filter, cancellationToken);

        await mediator.Publish(new SubcategoryMonthOutcomesReadEvent()
        {
            SessionId = request.SessionId,
            LastSentMessageId = request.LastSentMessageId,
            
            Category = query.Category,
            Subcategory = query.SubCategory,
            MonthFrom = query.MonthRange.From,
            
            Outcomes = outcomes

        }, cancellationToken);
    }
}

public record SubcategoryMonthOutcomesReadEvent : INotification
{
    public long SessionId { get; init; }
    public int LastSentMessageId { get; init; }
    
    public Category Category { get; init; }
    public SubCategory Subcategory { get; init; }
    public YearMonth MonthFrom { get; init; }
    public IReadOnlyList<Outcome> Outcomes { get; init; }
}

public class SubcategoryMonthOutcomesReadTableEventHandler(IMessageService messageService, ILogger<SubcategoryMonthOutcomesReadTableEventHandler> logger) : INotificationHandler<SubcategoryMonthOutcomesReadEvent>
{
    public async Task Handle(SubcategoryMonthOutcomesReadEvent notification, CancellationToken cancellationToken)
    {
        if (!notification.Outcomes.Any())
        {
            return;
        }

        var currencies = notification.Outcomes.Select(c => c.Amount.Currency).Distinct().ToArray();
        
        var expenseAggregator = new ExpensesAggregator<DateOnly>(e => e.Date.FirstDayOfMonth(), false, sortAsc: true);
        var statistic = expenseAggregator.Aggregate(notification.Outcomes, currencies);

        var tableOptions = new TableOptions()
        {
            Subtitle = $"Category: {notification.Category.Name}{Environment.NewLine}" +
                       $"Subcategory: {notification.Subcategory.Name}{Environment.NewLine}" +
                       $"Expenses from {notification.MonthFrom.ToString(DateFormat.FullMonthName)}",
            FirstColumnName = "Month"
        };
        
        var statisticW = StatisticMapper.Map(statistic, new DateOnlyColumnFactory());

        var table = StatisticTableBuilder.BuildTable(statisticW, tableOptions);
                
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

public class SubcategoryMonthOutcomesReadDiagramEventHandler(IPictureGenerator pictureGenerator, IMessageService messageService, ILogger<SubcategoryMonthOutcomesReadTableEventHandler> logger) : INotificationHandler<SubcategoryMonthOutcomesReadEvent>
{
    public async Task Handle(SubcategoryMonthOutcomesReadEvent notification, CancellationToken cancellationToken)
    {
        if (!notification.Outcomes.Any())
        {
            return;
        }

        var currencies = notification.Outcomes.Select(c => c.Amount.Currency).Distinct().ToArray();
        
        var expenseAggregator = new ExpensesAggregator<DateOnly>(e => e.Date.FirstDayOfMonth(), false, sortAsc: true);
        var statistic = expenseAggregator.Aggregate(notification.Outcomes, currencies);

        foreach (var currency in currencies)
        {
            var data = statistic.Rows.Select(r => (YearMonth.From(r.Row), r[currency].Amount)).ToList();
            
            var pictureBytes = pictureGenerator.GeneratePlot(data, currency, new PictureOptions($"{notification.Category.Name} {notification.Subcategory.Name} expenses since {notification.MonthFrom.ToString(DateFormat.FullMonthName)}"));
            
            await messageService.SendPictureAsync(
                new Message()
                {
                    ChatId = notification.SessionId,
                    PictureBytes = pictureBytes 
                }, cancellationToken
            );
        }
    }
}