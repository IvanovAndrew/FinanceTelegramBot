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

    public Task<List<IExpense>> ReadOutcomes(FinanseFilter finanseFilter, CancellationToken cancellationToken)
    {
        var result = 
            _savedExpenses.Where(expense =>
                    (finanseFilter.DateFrom == null || finanseFilter.DateFrom.Value <= expense.Date) &&
                    (finanseFilter.DateTo == null || expense.Date <= finanseFilter.DateTo.Value) &&
                    (finanseFilter.Category == null || finanseFilter.Category == expense.Category) &&
                    (finanseFilter.Subcategory == null || finanseFilter.Subcategory == expense.SubCategory) &&
                    (finanseFilter.Currency == null || finanseFilter.Currency == expense.Amount.Currency)

                )
                .ToList();

        return Task.FromResult(result);
    }

    public Task<List<IIncome>> ReadIncomes(FinanseFilter finanseFilter, CancellationToken cancellationToken)
    {
        var result = 
            _savedIncomes.Where(expense =>
                    (finanseFilter.DateFrom == null || finanseFilter.DateFrom.Value <= expense.Date) &&
                    (finanseFilter.DateTo == null || expense.Date <= finanseFilter.DateTo.Value) &&
                    (finanseFilter.Category == null || finanseFilter.Category == expense.Category) &&
                    (finanseFilter.Currency == null || finanseFilter.Currency == expense.Amount.Currency)

                )
                .ToList();

        return Task.FromResult(result);
    }
}