using Domain;

namespace Application.Test.Stubs;

public class FinanceRepositoryStub : IFinanceRepository
{
    private readonly List<Outcome> _savedExpenses = new();
    private readonly List<Income> _savedIncomes = new();
    public TimeSpan DelayTime { get; set; } = TimeSpan.Zero;

    public Task<SaveResult> SaveIncome(Income income, CancellationToken cancellationToken)
    {
        _savedIncomes.Add(income);
        return Task.FromResult(SaveResult.Ok());
    }

    public async Task<SaveResult> SaveAllOutcomes(IReadOnlyCollection<Outcome> expenses, CancellationToken cancellationToken)
    {
        await Task.Delay(DelayTime, cancellationToken);
        _savedExpenses.AddRange(expenses);

        return SaveResult.Ok();
    }

    public Task<List<Outcome>> ReadOutcomes(FinanceFilter financeFilter, CancellationToken cancellationToken)
    {
        var result = 
            _savedExpenses.Where(expense =>
                    (financeFilter.DateFrom == null || financeFilter.DateFrom.Value <= expense.Date) &&
                    (financeFilter.DateTo == null || expense.Date <= financeFilter.DateTo.Value) &&
                    (financeFilter.Category == null || financeFilter.Category == expense.Category) &&
                    (financeFilter.Subcategory == null || financeFilter.Subcategory == expense.SubCategory) &&
                    (financeFilter.Currency == null || financeFilter.Currency == expense.Amount.Currency)

                )
                .ToList();

        return Task.FromResult(result);
    }

    public Task<List<Income>> ReadIncomes(FinanceFilter financeFilter, CancellationToken cancellationToken)
    {
        var result = 
            _savedIncomes.Where(expense =>
                    (financeFilter.DateFrom == null || financeFilter.DateFrom.Value <= expense.Date) &&
                    (financeFilter.DateTo == null || expense.Date <= financeFilter.DateTo.Value) &&
                    (financeFilter.Category == null || financeFilter.Category == expense.Category) &&
                    (financeFilter.Currency == null || financeFilter.Currency == expense.Amount.Currency)

                )
                .ToList();

        return Task.FromResult(result);
    }
}