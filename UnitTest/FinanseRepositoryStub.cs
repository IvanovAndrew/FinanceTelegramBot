using Domain;
using Infrastructure;

namespace UnitTest;

public class FinanseRepositoryStub : IFinanseRepository
{
    private readonly List<IExpense> _savedExpenses = new();
    private readonly List<IIncome> _savedIncomes = new();
    public TimeSpan DelayTime { get; set; } = TimeSpan.Zero;

    public Task<bool> SaveIncome(IIncome income, CancellationToken cancellationToken)
    {
        _savedIncomes.Add(income);
        return Task.FromResult(true);
    }

    public async Task<bool> SaveAllOutcomes(List<IExpense> expenses, CancellationToken cancellationToken)
    {
        await Task.Delay(DelayTime, cancellationToken);
        _savedExpenses.AddRange(expenses);

        return true;
    }

    public Task<List<IExpense>> ReadOutcomes(FinanceFilter financeFilter, CancellationToken cancellationToken)
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

    public Task<List<IIncome>> ReadIncomes(FinanceFilter financeFilter, CancellationToken cancellationToken)
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