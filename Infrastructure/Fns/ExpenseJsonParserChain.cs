using Application;
using Domain;

namespace Infrastructure.Fns;

public sealed class ExpenseJsonParserChain : IExpenseJsonParser
{
    private readonly IReadOnlyList<IExpenseJsonParser> _parsers;

    public ExpenseJsonParserChain(IEnumerable<IExpenseJsonParser> parsers)
    {
        _parsers = parsers.ToList();
    }

    public Currency Currency { get; }
    public bool CanParse(string json) => true;
    public IReadOnlyList<Outcome> ParseOutcomes(string json, Category defaultCategory)
    {
        foreach (var parser in _parsers)
        {
            if (!parser.CanParse(json))
                continue;

            return parser.ParseOutcomes(json, defaultCategory);
        }

        throw new InvalidOperationException("No suitable JSON parser found.");
    }
}