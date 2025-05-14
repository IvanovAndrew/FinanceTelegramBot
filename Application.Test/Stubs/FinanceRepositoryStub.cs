using Domain;
using Infrastructure;

namespace UnitTest;

public class FinanceRepositoryStub : IFinanceRepository
{
    private readonly List<IMoneyTransfer> _savedExpenses = new();
    private readonly List<IMoneyTransfer> _savedIncomes = new();
    public TimeSpan DelayTime { get; set; } = TimeSpan.Zero;

    public Task<bool> SaveIncome(IMoneyTransfer income, CancellationToken cancellationToken)
    {
        _savedIncomes.Add(income);
        return Task.FromResult(true);
    }

    public async Task<bool> SaveAllOutcomes(IReadOnlyCollection<IMoneyTransfer> expenses, CancellationToken cancellationToken)
    {
        await Task.Delay(DelayTime, cancellationToken);
        _savedExpenses.AddRange(expenses);

        return true;
    }

    public Task<List<IMoneyTransfer>> ReadOutcomes(FinanceFilter financeFilter, CancellationToken cancellationToken)
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

    public Task<List<IMoneyTransfer>> ReadIncomes(FinanceFilter financeFilter, CancellationToken cancellationToken)
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