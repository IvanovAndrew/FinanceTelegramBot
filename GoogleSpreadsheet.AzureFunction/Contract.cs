namespace GoogleSpreadsheet;

[Serializable]
public class MoneyTransferDto
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

[Serializable]
public class CurrencyExchangeDto
{
    public DateOnly Date { get; set; }
    public string? Shop { get; set; }
    public string? Description { get; set; }
    public decimal SourceAmount { get; set; }
    public string SourceCurrency { get; set; }
    public decimal TargetAmount { get; set; }
    public string TargetCurrency { get; set; }
}

[Serializable]
public class FutureExpenseDto
{
    public string Name { get; set; }
    public string Category { get; set; }
    public string? Subcategory { get; set; }
    public string? Shop { get; set; }
    public string Frequency { get; set; }
    public string Way { get; set; }
    public decimal? Amount { get; set; }
    public string Currency { get; set; }
}