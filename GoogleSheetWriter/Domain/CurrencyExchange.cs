namespace GoogleSheetWriter;

public class CurrencyExchange
{
    public DateOnly Date { get; set; }
    public string? Shop { get; set; }
    public string? Description { get; set; }
    public decimal SourceAmount { get; set; }
    public string SourceCurrency { get; set; }
    public decimal TargetAmount { get; set; }
    public string TargetCurrency { get; set; }
}