using Application.Core.Events;
using Domain;
using MediatR;

namespace Application.Bot.Statistic;

public abstract class StatisticQueryHandlerBase<TRequest>(
    IFinanceRepository financeRepository, IMediator mediator) : IRequestHandler<TRequest>
    where TRequest : IRequest
{
    protected abstract long GetSessionId(TRequest request);
    protected abstract FinanceFilter BuildFilter(TRequest request);
    protected abstract INotification BuildReadEvent(TRequest request, IReadOnlyList<Outcome> outcomes);

    public async Task Handle(TRequest request, CancellationToken ct)
    {
        try
        {
            var outcomes = await financeRepository.ReadOutcomes(BuildFilter(request), ct);
            if (outcomes.Any())
                await mediator.Publish(BuildReadEvent(request, outcomes), ct);
        }
        catch (OperationCanceledException)
        {
            await mediator.Publish(new TaskCanceledEvent { SessionId = GetSessionId(request) }, ct);
        }
    }
}