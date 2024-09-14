namespace Domain
{
    public class Expense : IExpense
    {
        public DateOnly Date { get; init; }
        public string Category { get; init; }
        public string? SubCategory { get; init; }
        public string? Description { get; init; }
        public Money Amount { get; init; }
    }
    
    public class Income : IIncome
    {
        public DateOnly Date { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public Money Amount { get; set; }
    }
}