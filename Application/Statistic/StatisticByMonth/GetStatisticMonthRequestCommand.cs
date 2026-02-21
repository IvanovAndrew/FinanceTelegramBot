using Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Statistic.StatisticByMonth;

public record GetStatisticMonthRequestCommand : IRequest
{
    public long SessionId { get; init; }
    public int LastSentMessageId { get; init; }
    public StatisticsQuery Query { get; init; }
}

public class GetStatisticMonthRequestCommandHandler(IUserSessionService userSessionService, IFinanceRepository financeRepository, IMediator mediator, ILogger<GetStatisticMonthRequestCommandHandler> logger) : IRequestHandler<GetStatisticMonthRequestCommand>
{
    public async Task Handle(GetStatisticMonthRequestCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation($"{nameof(GetStatisticMonthRequestCommandHandler)} called");

        var filter = new FinanceFilter()
        {
            DateFrom = request.Query.MonthRange.From.ToDateOnly(),
            DateTo = request.Query.MonthRange.To.ToLastDayOfMonth(),
            Currency = request.Query.Currency,
        };

        var outcomes = await financeRepository.ReadOutcomes(filter, cancellationToken);
        await mediator.Publish(new MonthOutcomesReadEvent()
        {
            SessionId = request.SessionId,
            LastSentMessageId = request.LastSentMessageId,
            Month = request.Query.MonthRange.From,
            Outcomes = outcomes
        }, cancellationToken);
    }
}