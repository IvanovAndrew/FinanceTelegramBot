namespace Domain
{
    public class ExpensesAggregator<T>
    {
        private readonly Func<IMoneyTransfer, T> _sumBy;
        private readonly bool _sortByMoney;
        private readonly bool _sortAsc;

        public ExpensesAggregator(Func<IMoneyTransfer, T> sumBy, bool sortByMoney, bool sortAsc = false)
        {
            _sumBy = sumBy;
            _sortByMoney = sortByMoney;
            _sortAsc = sortAsc;
        }

        public Statistic<T> Aggregate(IEnumerable<IMoneyTransfer> expenses, IEnumerable<Currency> currencies)
        {
            var statistic = new Statistic<T>(currencies, _sumBy);

            foreach (var expense in expenses)
            {
                statistic.ProcessExpense(expense);
            }
            
            statistic.Sort(_sortByMoney, _sortAsc);

            return statistic;
        }
    }
    
    

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

    public class ExpenseInfo<T>
    {
        protected readonly Dictionary<Currency, int> Currencies;
        internal Money[] Money { get; }
        public T Row { get; init; }

        public ExpenseInfo(T row, Dictionary<Currency, int> currencies)
        {
            Row = row;
            Currencies = currencies;
            Money = new Money[currencies.Count];
            foreach (var currency in currencies)
            {
                Money[currency.Value] = new Money() { Amount = 0, Currency = currency.Key };
            }
        }

        public Money this[Currency currency] => Money[Currencies[currency]];

        public void Add(Money money)
        {
            var index = Currencies[money.Currency];
            Money[index] += money;
        }
    }

    public class TotalExpenseInfo<T> : ExpenseInfo<T>
    {
        public TotalExpenseInfo(T row, Dictionary<Currency, int> currencies) : base(row, currencies)
        {
        }

        public void Aggregate(List<ExpenseInfo<T>> rows)
        {
            foreach (var row in rows)
            {
                foreach (var value in Currencies.Values)
                {
                    this.Money[value] += row.Money[value];
                }
            }
        }
    }
} 