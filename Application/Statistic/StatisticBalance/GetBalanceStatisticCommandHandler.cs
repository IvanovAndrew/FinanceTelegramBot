using Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Statistic.StatisticBalance;

public class GetBalanceStatisticCommandHandler(IUserSessionService userSessionService, IFinanceRepository financeRepository, IMediator mediator, ILogger<GetBalanceStatisticCommandHandler> logger) : IRequestHandler<GetBalanceStatisticCommand>
{
    public async Task Handle(GetBalanceStatisticCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            await mediator.Publish(new BalanceStatisticCollectingStarted(){SessionId = session.Id, LastSentMessageId = (int) session.LastSentMessageId!}, cancellationToken);
            
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
                using (cancellationTokenSource = new CancellationTokenSource())
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
                    Money monthOutcomes = new Money() { Amount = 0, Currency = financeFilter.Currency ?? Currency.Rur };

                    foreach (var expense in expenses)
                    {
                        monthOutcomes += expense.Amount;
                    }

                    Money monthIncomes = new Money() { Amount = 0, Currency = financeFilter.Currency ?? Currency.Rur };
                    foreach (var income in incomes)
                    {
                        if (income.Amount.Currency != financeFilter.Currency || income.Date < financeFilter.DateFrom)
                        {
                            continue;
                        }

                        monthIncomes += income.Amount;
                    }

                    var table = new Table()
                    {
                        Title = "Balance",
                        Subtitle = $"From {financeFilter.DateFrom?.ToString("MMMM yyyy")}",
                        FirstColumnName = "Balance",
                        Currencies = new []{financeFilter.Currency!}
                    };
                    table.AddRow(new Row()
                    {
                        FirstColumnValue = "Income",
                        CurrencyValues = new Dictionary<Currency, Money>()
                            { [session.StatisticsOptions.Currency] = monthIncomes }
                    });
                    table.AddRow(new Row()
                    {
                        FirstColumnValue = "Outcome",
                        CurrencyValues = new Dictionary<Currency, Money>() { [session.StatisticsOptions.Currency] = monthOutcomes }
                    });
                    table.AddRow(new Row());
                    table.AddRow(new Row()
                    {
                        FirstColumnValue = "Total",
                        CurrencyValues = new Dictionary<Currency, Money>()
                            { [session.StatisticsOptions.Currency] = monthIncomes - monthOutcomes }
                    });

                    await mediator.Publish(new BalanceStatisticCalculatedEvent()
                    {
                        SessionId = session.Id,
                        LastSentMessageId = (int)session.LastSentMessageId!,
                        Table = table
                    }, cancellationToken);

                    session.LastSentMessageId = null;
                    session.QuestionnaireService = null;
                }
                else
                {
                    await mediator.Publish(new NeitherIncomesNotOutcomesFoundEvent()
                        { SessionId = session.Id, LastSentMessageId = (int) session.LastSentMessageId! });
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Operation is canceled by a user");
                await mediator.Publish(new LongOperationCanceledEvent() { SessionId = session.Id }, cancellationToken);
            }
            finally
            {
                cancellationTokenSource = null;
            }
        }
    }
}