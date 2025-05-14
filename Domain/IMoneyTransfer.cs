namespace Domain
{
    public interface IMoneyTransfer
    {
        bool IsIncome { get; }
        DateOnly Date { get; }
        Category Category { get; }
        SubCategory? SubCategory { get; }
        string? Description { get; }
        Money Amount { get; }
    }
}