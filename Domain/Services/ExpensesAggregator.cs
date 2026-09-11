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