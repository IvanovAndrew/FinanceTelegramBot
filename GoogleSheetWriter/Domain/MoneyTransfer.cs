namespace GoogleSheetWriter;

public class MoneyTransfer
{
    public bool IsIncome { get; set; }
    public DateOnly Date { get; set; }
    public string Category { get; set; }
    public string? Subcategory { get; set; }
    public string? Shop { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
}