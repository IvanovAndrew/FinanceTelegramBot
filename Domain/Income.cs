namespace Domain;

public class Income : IMoneyTransfer
{
    public bool IsIncome => true;
    public DateOnly Date { get; set; }
    public Category Category { get; set; }
    public SubCategory? SubCategory { get; }
    public string? Description { get; set;}
    public Money Amount { get; set; }
        
    public override string ToString()
    {
        return string.Join($"{Environment.NewLine}",
            "Income",
            $"Date: {Date:dd.MM.yyyy}",
            $"Category: {Category.Name}",
            $"Description: {Description ?? string.Empty}",
            $"Amount: {Amount}",
            "");
    }
}