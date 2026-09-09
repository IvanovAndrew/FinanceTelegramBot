namespace GoogleSheetWriter.Infrastructure;

public class GoogleRequestOptions
{
    internal string Range { get; init; }
    internal ExcelColumn[] RequestedColumns { get; init; }
}