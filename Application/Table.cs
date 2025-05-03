using Domain;

namespace Application;

public class Table
{
    public string Title { get; init; }
    public string Subtitle { get; init; }
    public string FirstColumnName { get; init; }
    public IReadOnlyList<Currency> Currencies { get; init; }
    public IReadOnlyList<Row> Rows => _rows;

    private List<Row> _rows = new List<Row>();

    public IReadOnlyList<string> ColumnNames
    {
        get
        {
            var columns = new List<string>();
            columns.Add(FirstColumnName);
            columns.AddRange(Currencies.Select(c => c.Name));

            return columns;
        }
    } 

    public void AddRow(Row row)
    {
        _rows.Add(row);
    }
}

public class Row
{
    public string FirstColumnValue { get; set; } = string.Empty;
    public Dictionary<Currency, Money> CurrencyValues { get; set; } = new();
}