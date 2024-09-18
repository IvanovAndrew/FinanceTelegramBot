namespace Domain
{
    public interface IMoneyTransfer
    {
        bool IsIncome { get; }
        DateOnly Date { get; }
        string Category { get; }
        string? SubCategory { get; }
        string? Description { get; }
        Money Amount { get; }
    }
}