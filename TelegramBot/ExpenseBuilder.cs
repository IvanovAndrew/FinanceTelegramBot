using Domain;

namespace TelegramBot;

internal class ExpenseBuilder
{
    internal DateOnly? Date { get; set; }
    internal string? Category { get; set; }
    internal string? SubCategory { get; set; }

    internal string? Description { get; set; }
    internal Money? Sum { get; set; }

    public IExpense Build()
    {
        return new Expense
        {
            Date = Date.Value,
            Category = Category,
            SubCategory = SubCategory,
            Description = Description,
            Amount = Sum!,
        };
    }
}