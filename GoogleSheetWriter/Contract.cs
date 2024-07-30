﻿namespace GoogleSheetWriter;

[Serializable]
public class Expense
{
    public DateTime Date { get; set; }
    public string Category { get; set; }
    public string? Subcategory { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
}

public enum Currency
{
    RUR,
    AMD, 
    GEL,
    USD,
    EUR
}