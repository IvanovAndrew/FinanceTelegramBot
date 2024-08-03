namespace GoogleSheetWriter;

[Serializable]
public class ExpenseSearchOption
{
    public DateTime? DateFrom { get; set; } = null;
    public DateTime? DateTo { get; set; } = null;
    public string Category { get; set; } = string.Empty;
    public string SubCategory { get; set; } = string.Empty;
    public Currency? Currency { get; set; } = null;

    internal bool IsSatisfied(Expense expense)
    {
        return (DateFrom == null || DateFrom.Value <= expense.Date) &&
               (DateTo == null || expense.Date <= DateTo.Value) &&
               (string.IsNullOrEmpty(Category) || expense.Category == Category) &&
               (string.IsNullOrEmpty(SubCategory) || expense.Category == SubCategory) &&
               (Currency == null || expense.Currency == Currency);
    }
}