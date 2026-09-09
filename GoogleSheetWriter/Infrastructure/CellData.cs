namespace GoogleSheetWriter.Infrastructure;

public record struct CellData
{
    public bool Filled { get; set; }
    public string? Value { get; set;}
}