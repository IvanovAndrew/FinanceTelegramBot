using Application.Core.Services;
using Application.Core.Statistic;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Bot.Statistic;

public class GetBalanceStatisticCommandHandler(
    IBalanceStatisticService balanceStatisticService,
    IMediator mediator,
    ILogger<GetBalanceStatisticCommandHandler> logger) : IRequestHandler<GetBalanceStatisticCommand>
{
    public async Task Handle(GetBalanceStatisticCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Started {Command}", request);

        await mediator.Publish(new BalanceStatisticCollectingStarted
        {
            SessionId = request.SessionId,
        }, cancellationToken);

        try
        {
            var result = await balanceStatisticService.Calculate(request.Query.MonthRange.From, request.Query.Currency, cancellationToken);

            await mediator.Publish(new BalanceStatisticCalculatedEvent
            {
                SessionId = request.SessionId,
                Currency = result.Currency,
                MonthBalances = result.MonthBalances,
                FutureExpenses = result.FutureExpenses,
                Saldo = result.Saldo,
                DailyBudget = result.DailyBudget,
                MonthRange = result.MonthRange,
                PeriodLeft = result.PeriodLeft,
            }, cancellationToken);
        }
        catch (NoFinanceDataException)
        {
            await mediator.Publish(new NeitherIncomesNotOutcomesFoundEvent
            {
                SessionId = request.SessionId,
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            await mediator.Publish(new LongOperationCanceledEvent
            {
                SessionId = request.SessionId
            }, cancellationToken);
        }

        logger.LogInformation("Finished");
    }
}