using Domain;

namespace Application;

public interface IExpenseJsonParser
{
    IReadOnlyList<Outcome> ParseOutcomes(string text, Category category, Currency currency);
}