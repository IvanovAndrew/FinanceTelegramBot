using Domain;

namespace Application.Core;

public interface IExpenseJsonParser
{
    public bool CanParse(string json);
    Check ParseCheck(string json);
}

public sealed class ExpenseJsonParserChain : IExpenseJsonParser
{
    private readonly IReadOnlyList<IExpenseJsonParser> _parsers;

    public ExpenseJsonParserChain(IEnumerable<IExpenseJsonParser> parsers)
    {
        _parsers = parsers.ToList();
    }

    public Currency Currency { get; }
    public bool CanParse(string json) => true;
    public Check ParseCheck(string json)
    {
        foreach (var parser in _parsers)
        {
            if (!parser.CanParse(json))
                continue;

            return parser.ParseCheck(json);
        }

        throw new InvalidOperationException("No suitable JSON parser found.");
    }
}