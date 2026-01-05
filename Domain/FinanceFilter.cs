namespace Domain;

public record FinanceFilter
{
    public bool Income { get; init; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public Category? Category { get; set; }
    public SubCategory? Subcategory { get; set; }
    public Currency? Currency { get; set; }
}