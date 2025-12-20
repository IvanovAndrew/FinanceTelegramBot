namespace Domain.Services;

public interface IRecurringExpensesService
{
    /// <summary>
    /// </summary>
    IReadOnlyList<Money> GetMissingRecurringExpenses(IEnumerable<IMoneyTransfer> allExpenses, DateOnly today);
}

public class RecurringExpensesService : IRecurringExpensesService
{
    public IReadOnlyList<Money> GetMissingRecurringExpenses(IEnumerable<IMoneyTransfer> allExpenses, DateOnly today)
    {
        var monthAgo = today.AddMonths(-1);
        var previousMonth = allExpenses
            .Where(e => e.SubCategory?.IsRecurringMonthly == true &&
                        e.Date.Year == monthAgo.Year && e.Date.Month == monthAgo.Month)
            .ToList();

        var currentMonth = allExpenses
            .Where(e => e.SubCategory?.IsRecurringMonthly == true &&
                        e.Date.Year == today.Year && e.Date.Month == today.Month)
            .ToList();

        var missing = previousMonth
            .Where(prev => !currentMonth.Any(curr =>
                curr.Category == prev.Category && curr.SubCategory == prev.SubCategory))
            .Select(e => e.Amount)
            .ToList();

        return missing;
    }
}