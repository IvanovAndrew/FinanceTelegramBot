namespace Domain;

public class Statistic<T>
{
    private readonly Dictionary<Currency, int> _currencyToIndex = new();
    private readonly Dictionary<T, ExpenseInfo<T>> _rowsDict = new();
    private readonly Func<IMoneyTransfer, T> _sumBy;
    private List<ExpenseInfo<T>> _rows = new();

    public Statistic(IEnumerable<Currency> currencies, Func<IMoneyTransfer, T> sumBy)
    {
        int i = 0;
        foreach (var currency in currencies)
        {
            _currencyToIndex[currency] = i++;
        }

        _sumBy = sumBy?? throw new ArgumentNullException(nameof(sumBy));
    }

    public List<ExpenseInfo<T>> Rows => _rows;
    public IReadOnlyList<Currency> Currencies => _currencyToIndex.Keys.ToList();

    private TotalExpenseInfo<T> _total; 
    public TotalExpenseInfo<T> Total
    {
        get
        {
            if (_total == null)
            {
                _total = new TotalExpenseInfo<T>(default, _currencyToIndex);
                _total.Aggregate(Rows);
            }
                
            return _total;
        }
    }
        

    internal void ProcessExpense(IMoneyTransfer expense)
    {
        if (!_currencyToIndex.TryGetValue(expense.Amount.Currency, out var index)) return;

        var rowName = _sumBy(expense);
        if (!_rowsDict.TryGetValue(rowName, out var row))
        {
            _rowsDict[rowName] = row = new ExpenseInfo<T>(rowName, _currencyToIndex);
            _rows.Add(row);
        }
            
        row.Add(expense.Amount);
    }

    internal void Sort(bool sortByMoney, bool sortAsc)
    {
        if (sortByMoney)
        {
            var firstCurrency = _currencyToIndex.MinBy(c => c.Value).Key;
            if (sortAsc)
            {
                _rows = _rowsDict.Values.OrderBy(e => e[firstCurrency]).ToList();
            }
            else
            {
                _rows = _rowsDict.Values.OrderByDescending(e => e[firstCurrency]).ToList();
            }
        }
        else
        {
            if (sortAsc)
            {
                _rows = _rowsDict.Values.OrderBy(e => e.Row).ToList();
            }
            else
            {
                _rows = _rowsDict.Values.OrderByDescending(e => e.Row).ToList();
            }
        }
    }
}