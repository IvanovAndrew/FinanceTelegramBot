using System.Collections.Generic;

namespace Domain
{
    public interface ISpreadsheetOptions
    {
        string Url { get; init; }
        List<IExcelList> ExcelLists { get; init; }
    }

    public interface IExcelList
    {
        IExcelListStructure Usual { get; init; }
        IExcelListStructure Home { get; init; }
        IExcelListStructure BigDeal { get; init; }
        IExcelListStructure Income { get; init; }
    }

    public interface IExcelListStructure
    {
        string Name { get; init; }
        Dictionary<string, string> Columns { get; init; }
    }
}