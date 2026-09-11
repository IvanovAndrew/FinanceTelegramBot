using Application.Core.Services;
using Domain;
using Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Api;

public class GetSpendingHistoryApiCommandHandler(
    ICurrencyExchangeOutcomeMatcher exchangeMatcher,
    IFinanceRepository financeRepository,
    IDateTimeService dateTimeService, ILogger<GetSpendingHistoryApiCommandHandler> logger)
    : IRequestHandler<GetSpendingHistoryApiCommand, SpendingHistoryResult>
{
    public async Task<SpendingHistoryResult> Handle(GetSpendingHistoryApiCommand request, CancellationToken cancellationToken)
    {
        var currency = request.Currency;
        var today = dateTimeService.Today();
        var startDate = request.StartMonth.ToDateOnly(1);
        var monthRange = new MonthRange { From = request.StartMonth, To = YearMonth.From(today) };

        var filter = new FinanceFilter { Currency = currency, DateFrom = startDate };
        var currencyExchanges = await financeRepository.ReadCurrencyExchanges(currency, startDate, today, cancellationToken);
        var outcomes = await financeRepository.ReadOutcomes(filter, cancellationToken);
        var incomes = await financeRepository.ReadIncomes(filter, cancellationToken);

        var exchangesFromCurrency = currencyExchanges.Where(x => x.SourceAmount.Currency == currency).ToList();
        var foreignCurrencies = exchangesFromCurrency.Select(x => x.TargetAmount.Currency).Distinct();

        var foreignOutcomesByCurrency = new Dictionary<Currency, IReadOnlyCollection<Outcome>>();
        foreach (var fc in foreignCurrencies)
        {
            var fo = await financeRepository.ReadOutcomes(new FinanceFilter { Currency = fc, DateFrom = startDate }, cancellationToken);
            foreignOutcomesByCurrency[fc] = fo.ToList();
        }

        var matchedOutcomes = exchangeMatcher.Match(exchangesFromCurrency, foreignOutcomesByCurrency);

        var outcomesByMonth = outcomes
            .Union(matchedOutcomes)
            .ToLookup(o => YearMonth.From(o.Date));
        
        var incomesByMonth = incomes
                                                        .Union(currencyExchanges.Where(x => x.TargetAmount.Currency == currency).Select(x => new Income(){Date = x.Date, Category = Categories.Income.CurrencyExchange, Amount = x.TargetAmount}))
                                                        .ToLookup(i => YearMonth.From(i.Date));

        var months = new List<MonthSpending>();
        for (var month = monthRange.From; month <= monthRange.To; month = month.Next())
        {
            var outcomeCategories = BuildCategories(outcomesByMonth[month], currency);
            
            var savingsTotal = Sum(outcomeCategories.Where(c => c.Category == Categories.Outcome.Savings).Select(c => c.Total), currency);
            
            var monthOutcomeTotal = Sum(outcomeCategories.Select(c => c.Total), currency);
            
            var incomeCategories = BuildCategories(incomesByMonth[month], currency);
            var monthIncomeTotal = Sum(incomeCategories.Select(c => c.Total), currency);

            months.Add(new MonthSpending(month, 
                monthIncomeTotal - monthOutcomeTotal, 
                monthOutcomeTotal,
                monthOutcomeTotal - savingsTotal,
                monthIncomeTotal, outcomeCategories, incomeCategories));
        }

        return new SpendingHistoryResult(currency, months);
    }

    internal static IReadOnlyList<CategorySpending> BuildCategories(IEnumerable<IMoneyTransfer> outcomes, Currency currency) =>
        outcomes
            .GroupBy(o => o.Category)
            .Select(g => new CategorySpending(
                g.Key,
                Sum(g.Select(o => o.Amount), currency),
                BuildSubCategories(g, currency)))
            .OrderByDescending(c => c.Total.Amount)
            .ToList();

    internal static IReadOnlyList<SubCategorySpending> BuildSubCategories(IEnumerable<IMoneyTransfer> outcomes, Currency currency) =>
        outcomes
            .GroupBy(o => o.SubCategory)   // null will be in the same bucket
            .Select(g => new SubCategorySpending(g.Key, Sum(g.Select(o => o.Amount), currency)))
            .OrderByDescending(s => s.Total.Amount)
            .ToList();

    internal static Money Sum(IEnumerable<Money> amounts, Currency currency) =>
        amounts.Aggregate(Money.Zero(currency), (acc, m) => acc + m);
}