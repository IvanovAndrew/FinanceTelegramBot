namespace GoogleSheetWriter.Domain;

public class FutureExpense
{
    public string Name { get; init; }
    public string Category { get; init; }
    public string? Subcategory { get; init; }
    public string? Shop { get; init; }
    
    public string Frequency { get; init; }
    public string Way { get; init; }
    
    public decimal? Amount { get; init; }
    public string Currency { get; init; }
}