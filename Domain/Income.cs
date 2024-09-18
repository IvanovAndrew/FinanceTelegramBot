namespace Domain;

public class Income : IMoneyTransfer
{
    public bool IsIncome => true;
    public DateOnly Date { get; set; }
    public string Category { get; set;}
    public string? SubCategory { get; set;}
    public string? Description { get; set;}
    public Money Amount { get; set; }
        
    public override string ToString()
    {
        return string.Join($"{Environment.NewLine}",
            "Income",
            $"Date: {Date:dd.MM.yyyy}",
            $"Category: {Category}",
            $"Description: {Description ?? string.Empty}",
            $"Amount: {Amount}",
            "");
    }
}