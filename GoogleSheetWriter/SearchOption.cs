namespace GoogleSheetWriter;

[Serializable]
public class SearchOption
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
               (string.IsNullOrEmpty(Category) || string.Equals(expense.Category, Category, StringComparison.InvariantCultureIgnoreCase)) &&
               (string.IsNullOrEmpty(SubCategory) || string.Equals(expense.Subcategory, SubCategory, StringComparison.InvariantCultureIgnoreCase)) &&
               (Currency == null || expense.Currency == Currency);
    }
}