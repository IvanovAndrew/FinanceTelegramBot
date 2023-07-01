namespace Domain
{
    public class ExpensesAggregator<T>
    {
        private readonly Func<IExpense, T> _sumBy;
        private readonly Func<T, string> _firstColumnName;
        private readonly bool _orderByMoney;
        private readonly bool _sortAsc;

        public ExpensesAggregator(Func<IExpense, T> sumBy, Func<T, string> firstColumnName, bool orderByMoney = true,
            bool sortAsc = true)
        {
            _sumBy = sumBy;
            _firstColumnName = firstColumnName;
            _orderByMoney = orderByMoney;
            _sortAsc = sortAsc;
        }

        public (List<(string, Money)>, Money) Aggregate(IEnumerable<IExpense> expenses, Currency currency)
        {
            var categoriesSum = new Dictionary<T, Money>();
            Money total = new() { Currency = currency, Amount = 0m };

            foreach (var row in expenses)
            {
                if (row.Amount.Currency != currency) continue;

                var key = _sumBy(row);
                if (categoriesSum.TryGetValue(key, out var sum))
                {
                    categoriesSum[key] = sum + row.Amount;
                }
                else
                {
                    categoriesSum[key] = row.Amount;
                }

                total += row.Amount;
            }

            IOrderedEnumerable<KeyValuePair<T, Money>> result;

            if (_orderByMoney)
            {
                if (_sortAsc)
                {
                    result = categoriesSum.OrderBy(kvp => kvp.Value);
                }
                else
                {
                    result = categoriesSum.OrderByDescending(kvp => kvp.Value);
                }
            }
            else
            {
                if (_sortAsc)
                {
                    result = categoriesSum.OrderBy(kvp => kvp.Key);
                }
                else
                {
                    result = categoriesSum.OrderByDescending(kvp => kvp.Key);
                }
            }

            return (result.Select(kvp => (_firstColumnName(kvp.Key), kvp.Value)).ToList(), total);
        }
    }
}