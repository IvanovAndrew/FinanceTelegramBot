using System;

namespace Domain
{
    public interface IExpense
    {
        DateOnly Date { get; }
        string Category { get; }
        string? SubCategory { get; }
        string? Description { get; }
        Money Amount { get; }
    }
}