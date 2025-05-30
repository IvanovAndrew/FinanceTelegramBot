namespace Domain
{
    public class Outcome : IMoneyTransfer
    {
        public bool IsIncome => false;
        public DateOnly Date { get; init; }
        public Category Category { get; init; }
        public SubCategory? SubCategory { get; init; }
        public string? Description { get; init; }
        public Money Amount { get; init; }

        public override string ToString()
        {
            return string.Join($"{Environment.NewLine}",
                "Outcome",
                $"Date: {Date:dd.MM.yyyy}",
                $"Category: {Category.Name}",
                $"Subcategory: {SubCategory?.Name.ToString()}",
                $"Description: {Description ?? string.Empty}",
                $"Amount: {Amount}",
                "");
        }
    }
}