using Application.AddMoneyTransfer;
using Application.Events;
using Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Statistic.StatisticByDay;

public class StatisticDayRequestCommandHandler(IUserSessionService userSessionService, IFinanceRepository financeRepository, IMediator mediator) : IRequestHandler<StatisticDayRequestCommand>
{
    public async Task Handle(StatisticDayRequestCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);
        if (session?.ActiveFlow is not StatisticsFlow flow) return;

        var filter = new FinanceFilter()
        {
            DateFrom = request.Query.Period.From,
            DateTo = request.Query.Period.From,
            Currency = request.Query.Currency,
        };

        try
        {
            var outcomes = await financeRepository.ReadOutcomes(filter, cancellationToken);

            if (outcomes.Any())
            {
                await mediator.Publish(new DayOutcomesReadEvent()
                {
                    Outcomes = outcomes,
                    
                    SessionId = request.SessionId,
                    LastSentMessageId = request.LastSentMessageId,
                    Day = request.Query.Period.From
                }, cancellationToken);
            }
        }
        catch (OperationCanceledException e)
        {
            await mediator.Publish(new TaskCanceledEvent(){SessionId = session.Id}, cancellationToken);
        }
    }
}

public record DayOutcomesReadEvent : INotification
{
    public long SessionId { get; init; }
    public int LastSentMessageId { get; init; }
    
    public IReadOnlyList<Outcome> Outcomes { get; init; }
    public DateOnly Day { get; init; }
}

public class DayOutcomesReadEventHandler(IMessageService messageService, ILogger<DayOutcomesReadEventHandler> logger) : INotificationHandler<DayOutcomesReadEvent>
{
    public async Task Handle(DayOutcomesReadEvent notification, CancellationToken cancellationToken)
    {
        if (!notification.Outcomes.Any())
        {
            return;
        }
        
        var currencies = notification.Outcomes.Select(c => c.Amount.Currency).Distinct().ToArray();
        var expenseAggregator = new ExpensesAggregator<string>(e => e.Category.Name, true, sortAsc: false);
        
        var statistic = expenseAggregator.Aggregate(notification.Outcomes, currencies);
        var wrapper = StatisticMapper.Map(statistic, new StringColumnFactory());

        var tableOptions = new TableOptions()
        {
            Subtitle = $"Expenses for {notification.Day.ToString("d MMMM yyyy")}", 
            FirstColumnName = "Category", 
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