using Domain;

namespace Application;

public interface IExpenseJsonParser
{
    protected Currency Currency { get; }
    public bool CanParse(string json);
    IReadOnlyList<Outcome> ParseOutcomes(string json, Category defaultCategory);
}