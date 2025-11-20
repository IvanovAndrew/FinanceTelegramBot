using Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Statistic.StatisticBalance;

public class GetBalanceStatisticCommandHandler(IUserSessionService userSessionService, IFinanceRepository financeRepository, IDateTimeService dateTimeService, IMediator mediator, ILogger<GetBalanceStatisticCommandHandler> logger) : IRequestHandler<GetBalanceStatisticCommand>
{
    public async Task Handle(GetBalanceStatisticCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation($"{nameof(GetBalanceStatisticCommandHandler)} started. {request}");
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session == null)
        {
            logger.LogWarning($"{nameof(GetBalanceStatisticCommandHandler)} session {request.SessionId} hasn't been found");
            return;
        }
        
        await mediator.Publish(new BalanceStatisticCollectingStarted(){SessionId = session.Id, LastSentMessageId = session.LastSentMessageId}, cancellationToken);
        
        var cancellationTokenSource = session.CancellationTokenSource = new CancellationTokenSource();

        try
        {
            List<IMoneyTransfer> expenses;
            List<IMoneyTransfer> incomes;

            var financeFilter = new FinanceFilter()
            {
                Currency = session.StatisticsOptions.Currency,
                DateFrom = session.StatisticsOptions.DateFrom
            };

            Task<List<IMoneyTransfer>>[] tasks;
            using (cancellationTokenSource)
            {
                tasks = new[]
                {
                    financeRepository.ReadOutcomes(financeFilter, cancellationTokenSource.Token),
                    financeRepository.ReadIncomes(financeFilter, cancellationTokenSource.Token)
                };

                var results = await Task.WhenAll(tasks);
                expenses = results[0];
                incomes = results[1];
            }

            logger.LogInformation($"{expenses.Count} expenses satisfy the requirements");
            logger.LogInformation($"{incomes.Count} incomes satisfy the requirements");

            if (expenses.Any() || incomes.Any())
            {
                Money monthOutcomes = new Money() { Amount = 0, Currency = financeFilter.Currency ?? Currency.RUR };

                foreach (var expense in expenses)
                {
                    monthOutcomes += expense.Amount;
                }

                Money monthIncomes = new Money() { Amount = 0, Currency = financeFilter.Currency ?? Currency.RUR };
                foreach (var income in incomes)
                {
                    if (income.Amount.Currency != financeFilter.Currency || income.Date < financeFilter.DateFrom)
                    {
                        continue;
                    }

                    monthIncomes += income.Amount;
                }

                string postTableInfo = string.Empty;
                
                var moneyLeft = await mediator.Send(new SpendingUntilPaydayCommand()
                {
                    SessionId = session.Id,
                    DateFrom = financeFilter.DateFrom.Value,
                    Currency = financeFilter.Currency!,
                    Balance = monthIncomes - monthOutcomes,
                }, cancellationToken);

                postTableInfo =
                    $"{moneyLeft.MoneyPerDay} can be spent daily till the payday {moneyLeft.Payday.ToString("d MMMM yyyy")}";

                var table = BuildTable(monthIncomes, monthOutcomes, financeFilter.DateFrom!.Value, financeFilter.Currency!, postTableInfo);
                await mediator.Publish(new BalanceStatisticCalculatedEvent()
                {
                    SessionId = session.Id,
                    LastSentMessageId = session.LastSentMessageId,
                    Table = table
                }, cancellationToken);

                session.LastSentMessageId = null;
                session.QuestionnaireService = null;

                
            }
            else
            {
                await mediator.Publish(new NeitherIncomesNotOutcomesFoundEvent()
                    { SessionId = session.Id, LastSentMessageId = session.LastSentMessageId }, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Operation is canceled by a user");
            await mediator.Publish(new LongOperationCanceledEvent() { SessionId = session.Id }, cancellationToken);
        }
        finally
        {
            cancellationTokenSource.Dispose();
        }
        
        logger.LogInformation($"{nameof(GetBalanceStatisticCommandHandler)} finishes");
    }

    private static Table BuildTable(Money monthIncomes, Money monthOutcomes, DateOnly dateFrom, Currency currency, string postTableInfo)
    {
        var table = new Table()
        {
            Title = "Balance",
            Subtitle = $"From {dateFrom.ToString("MMMM yyyy")}",
            FirstColumnName = "Balance",
            Currencies = new []{currency},
            PostTableInfo = postTableInfo
        };
        table.AddRow(new Row()
        {
            FirstColumnValue = "Income",
            CurrencyValues = new Dictionary<Currency, Money>()
                { [currency] = monthIncomes }
        });
        table.AddRow(new Row()
        {
            FirstColumnValue = "Outcome",
            CurrencyValues = new Dictionary<Currency, Money>() { [currency] = monthOutcomes }
        });
        table.AddRow(new Row());
        table.AddRow(new Row()
        {
            FirstColumnValue = "Total",
            CurrencyValues = new Dictionary<Currency, Money>()
                { [currency] = monthIncomes - monthOutcomes }
        });
        return table;
    }
}