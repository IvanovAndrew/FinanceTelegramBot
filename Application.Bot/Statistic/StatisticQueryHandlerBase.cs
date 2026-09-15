using Application.Core;
using Application.Core.Events;
using Domain;
using MediatR;

namespace Application.Bot.Statistic;

public abstract class StatisticQueryHandlerBase<TRequest>(
    IExpensesService expensesService, IMediator mediator) : IRequestHandler<TRequest>
    where TRequest : IRequest
{
    protected abstract long GetSessionId(TRequest request);
    protected abstract FinanceFilter BuildFilter(TRequest request);
    protected abstract INotification BuildReadEvent(TRequest request, IReadOnlyList<Outcome> outcomes);

    public async Task Handle(TRequest request, CancellationToken ct)
    {
        try
        {
            var outcomes = await expensesService.GetAllExpenses(BuildFilter(request), ct);
            if (outcomes.Any())
                await mediator.Publish(BuildReadEvent(request, outcomes), ct);
        }
        catch (OperationCanceledException)
        {
            await mediator.Publish(new TaskCanceledEvent { SessionId = GetSessionId(request) }, ct);
        }
    }
}