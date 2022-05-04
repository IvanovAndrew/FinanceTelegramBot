using System;

namespace Domain;

public interface IExpense
{
    DateTime Date { get; init; }
    string Category { get; init; }
    string? SubCategory { get; init; }
    string? Description { get; init; }
    decimal Amount { get; init;}
}

public class Expense : IExpense
{
    public DateTime Date { get; init; }
    public string Category { get; init; }
    public string? SubCategory { get; init; }
    public string? Description { get; init; }
    public decimal Amount { get; init; }
}