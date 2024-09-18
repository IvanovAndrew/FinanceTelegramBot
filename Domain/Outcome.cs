namespace Domain
{
    public class Outcome : IMoneyTransfer
    {
        public bool IsIncome => false;
        public DateOnly Date { get; init; }
        public string Category { get; init; }
        public string? SubCategory { get; init; }
        public string? Description { get; init; }
        public Money Amount { get; init; }

        public override string ToString()
        {
            return string.Join($"{Environment.NewLine}",
                "Outcome",
                $"Date: {Date:dd.MM.yyyy}",
                $"Category: {Category}",
                $"Subcategory: {SubCategory ?? string.Empty}",
                $"Description: {Description ?? string.Empty}",
                $"Amount: {Amount}",
                "");
        }
    }
}