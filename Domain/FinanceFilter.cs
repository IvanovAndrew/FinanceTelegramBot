namespace Domain;

public class FinanceFilter
{
    public bool Income { get; init; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public string? Category { get; set; }
    public string? Subcategory { get; set; }
    public Currency? Currency { get; set; }
}