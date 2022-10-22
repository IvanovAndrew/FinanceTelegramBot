using System;

namespace Domain;

public interface IExpense
{
    DateOnly Date { get; init; }
    string Category { get; init; }
    string? SubCategory { get; init; }
    string? Description { get; init; }
    Money Amount { get; init;}
}