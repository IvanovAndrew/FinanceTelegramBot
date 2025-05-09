using System.Globalization;
using GoogleSheetWriter;

public class GoogleDataWrapperTests
{
    private readonly CultureInfo _culture = new CultureInfo("ru-RU");

    private static IReadOnlyDictionary<ExcelColumn, CellData> CreateCellData(Dictionary<string, string?> values)
    {
        var dict = new Dictionary<ExcelColumn, CellData>();
        foreach (var (key, value) in values)
        {
            dict[ExcelColumn.FromString(key)] = new CellData {Filled = !string.IsNullOrEmpty(value), Value = value };
        }
        return dict;
    }

    private ListInfo CreateListInfo() => new ListInfo
    {
        DateColumn = ExcelColumn.FromString("C"),
        CategoryColumn = ExcelColumn.FromString("D"),
        SubCategoryColumn = ExcelColumn.FromString("E"),
        DescriptionColumn = ExcelColumn.FromString("F"),
        AmountRurColumn = ExcelColumn.FromString("G"),
        AmountAmdColumn = ExcelColumn.FromString("H"),
        AmountGelColumn = ExcelColumn.FromString("I"),
        AmountUsdColumn = ExcelColumn.FromString("J"),
        AmountEurColumn = ExcelColumn.FromString("K"),
    };

    [Fact]
    public void Date_ParsesCorrectly()
    {
        var cellData = CreateCellData(new()
        {
            ["C"] = "12.04.2024"
        });

        var wrapper = new GoogleDataWrapper(cellData, CreateListInfo(), _culture);
        Assert.Equal(new DateOnly(2024, 4, 12), wrapper.Date);
    }

    [Fact]
    public void Category_ReturnsFallbackIfMissing()
    {
        var cellData = CreateCellData(new()
        {
            ["D"] = null
        });

        var wrapper = new GoogleDataWrapper(cellData, CreateListInfo(), _culture);
        Assert.Equal("UNKNOWN", wrapper.Category);
    }

    [Fact]
    public void Amount_ParsesFirstNonZero()
    {
        var cellData = CreateCellData(new()
        {
            ["G"] = "",
            ["H"] = "",
            ["I"] = "500"
        });

        var wrapper = new GoogleDataWrapper(cellData, CreateListInfo(), _culture);
        Assert.Equal(500m, wrapper.Amount);
        Assert.Equal(Currency.GEL, wrapper.Currency);
    }

    [Fact]
    public void Currency_DetectsCorrectCurrency()
    {
        var cellData = CreateCellData(new()
        {
            ["I"] = "250,50"
        });

        var wrapper = new GoogleDataWrapper(cellData, CreateListInfo(), _culture);
        Assert.Equal(Currency.GEL, wrapper.Currency);
    }

    [Fact]
    public void Currency_DefaultsToRUR_IfEmpty()
    {
        var cellData = CreateCellData(new()
        {
            ["RUR"] = null,
            ["AMD"] = null,
            ["GEL"] = null,
            ["USD"] = null,
            ["EUR"] = null
        });

        var wrapper = new GoogleDataWrapper(cellData, CreateListInfo(), _culture);
        Assert.Equal(Currency.RUR, wrapper.Currency);
    }

    [Fact]
    public void Currency_ReturnsRUR_IfValueIsZagruzka()
    {
        var cellData = CreateCellData(new()
        {
            ["G"] = "Загрузка..."
        });

        var wrapper = new GoogleDataWrapper(cellData, CreateListInfo(), _culture);
        Assert.Equal(Currency.RUR, wrapper.Currency);
    }

    [Fact]
    public void SubCategory_ReturnsCorrectValue()
    {
        var cellData = CreateCellData(new()
        {
            ["E"] = "Food"
        });

        var wrapper = new GoogleDataWrapper(cellData, CreateListInfo(), _culture);
        Assert.Equal("Food", wrapper.SubCategory);
    }

    [Fact]
    public void Description_ReturnsCorrectValue()
    {
        var cellData = CreateCellData(new()
        {
            ["F"] = "Dinner with friends"
        });

        var wrapper = new GoogleDataWrapper(cellData, CreateListInfo(), _culture);
        Assert.Equal("Dinner with friends", wrapper.Description);
    }
}