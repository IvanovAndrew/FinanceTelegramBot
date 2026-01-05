using Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Statistic.StatisticBalance;

public class GetBalanceStatisticCommandHandler(
    IUserSessionService userSessionService,
    IBalanceStatisticService balanceStatisticService,
    IMediator mediator,
    ILogger<GetBalanceStatisticCommandHandler> logger) : IRequestHandler<GetBalanceStatisticCommand>
{
    public async Task Handle(GetBalanceStatisticCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Started {Command}", request);

        var session = userSessionService.GetUserSession(request.SessionId);
        if (session == null)
        {
            logger.LogWarning("Session {SessionId} not found", request.SessionId);
            return;
        }

        await mediator.Publish(new BalanceStatisticCollectingStarted
        {
            SessionId = session.Id,
            LastSentMessageId = session.LastSentMessageId
        }, cancellationToken);

        try
        {
            var result = await balanceStatisticService.Calculate(session.StatisticsOptions.DateFrom.Value, session.StatisticsOptions.Currency, cancellationToken);

            await mediator.Publish(new BalanceStatisticCalculatedEvent
            {
                SessionId = session.Id,
                LastSentMessageId = session.LastSentMessageId,
                DateFrom = result.DateFrom,
                Currency = result.Currency,
                IncludeToday = result.IncludeToday,
                MonthBalances = result.MonthBalances,
                MoneyLeft = result.MoneyLeft,
                SalaryDay = result.SalaryDay
            }, cancellationToken);

            session.LastSentMessageId = null;
            session.QuestionnaireService = null;
        }
        catch (NoFinanceDataException)
        {
            await mediator.Publish(new NeitherIncomesNotOutcomesFoundEvent
            {
                SessionId = session.Id,
                LastSentMessageId = session.LastSentMessageId
            }, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            await mediator.Publish(new LongOperationCanceledEvent
            {
                SessionId = session.Id
            }, cancellationToken);
        }

        logger.LogInformation("Finished");
    }
}