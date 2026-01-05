namespace Domain;

public class Income : IMoneyTransfer
{
    public bool IsIncome => true;
    public DateOnly Date { get; init; }
    public Category Category { get; init; }
    public SubCategory? SubCategory { get; }
    public string? Description { get; init;}
    public Money Amount { get; init; }
    
    public bool IsSalary() => Category.Type == CategoryType.Salary;
        
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