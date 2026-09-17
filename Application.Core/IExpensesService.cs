using Domain;
using Domain.Services;

namespace Application.Core;

public interface IExpensesService
{
    public Task<IReadOnlyList<Outcome>> GetAllExpenses(Currency currency, DateOnly dateFrom, DateOnly dateTo, CancellationToken cancellationToken);
    public Task<IReadOnlyList<Outcome>> GetAllExpenses(FinanceFilter filter, CancellationToken cancellationToken);
}

public class ExpensesService(IFinanceRepository financeRepository, ICurrencyExchangeOutcomeMatcher currencyExchangeOutcomeMatcher) : IExpensesService
{
    public async Task<IReadOnlyList<Outcome>> GetAllExpenses(Currency currency, DateOnly dateFrom, DateOnly dateTo, CancellationToken cancellationToken)
    {
        return await GetAllExpenses(new FinanceFilter()
        {
            DateFrom = dateFrom,
            DateTo = dateTo,
            Currency = currency,
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<Outcome>> GetAllExpenses(FinanceFilter filter, CancellationToken cancellationToken)
    {
        var outcomes = await financeRepository.ReadOutcomes(filter, cancellationToken);
        var currencyExchanges = await financeRepository.ReadCurrencyExchanges(filter, cancellationToken);
        
        var exchangesFromCurrency = currencyExchanges.Where(x => x.SourceAmount.Currency == filter.Currency).ToList();
        var foreignCurrencies = exchangesFromCurrency.Select(x => x.TargetAmount.Currency).Distinct();

        var foreignOutcomesByCurrency = new Dictionary<Currency, IReadOnlyCollection<Outcome>>();
        foreach (var fc in foreignCurrencies)
        {
            var fo = await financeRepository.ReadOutcomes(new FinanceFilter { Currency = fc, DateFrom = filter.DateFrom, DateTo = filter.DateTo, Category = filter.Category, Subcategory = filter.Subcategory}, cancellationToken);
            foreignOutcomesByCurrency[fc] = fo.ToList();
        }

        var matchedOutcomes = currencyExchangeOutcomeMatcher.Match(exchangesFromCurrency, foreignOutcomesByCurrency).Where(o => o.Matches(filter));
        
        return outcomes.Union(matchedOutcomes).ToList();
    }
}