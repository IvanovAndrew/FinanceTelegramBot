using System;
using Domain;

namespace TelegramBot;

internal class ExpenseBuilder
{
    internal DateOnly? Date { get; set; }
    internal Category? Category { get; set; }
    internal SubCategory? SubCategory { get; set; }

    internal string? Description { get; set; }
    internal Money? Sum { get; set; }

    public IExpense Build()
    {
        return new Expense
        {
            Date = Date.Value,
            Category = Category.Name,
            SubCategory = SubCategory?.Name,
            Description = Description,
            Amount = Sum!,
        };
    }
}