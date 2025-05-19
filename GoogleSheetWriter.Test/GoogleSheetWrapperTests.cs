using GoogleSheetWriter;

public class GoogleSheetWrapperTests
{
    [Fact]
    public async Task SaveIncome_WritesCorrectRangeAndValues()
    {
        // Arrange
        var googleService = new GoogleServiceStub();
        var options = new SheetOptions
        {
            Incomes = new ListInfo
            {
                ListName = "Incomes",
                YearColumn = ExcelColumn.FromString("A"),
                MonthColumn = ExcelColumn.FromString("B"),
                DateColumn = ExcelColumn.FromString("C"),
                CategoryColumn = ExcelColumn.FromString("D"),
                DescriptionColumn = ExcelColumn.FromString("E"),
                AmountRurColumn = ExcelColumn.FromString("F"),
                AmountAmdColumn = ExcelColumn.FromString("G"),
                AmountGelColumn = ExcelColumn.FromString("H"),
                AmountUsdColumn = ExcelColumn.FromString("I"),
                AmountEurColumn = ExcelColumn.FromString("J"),
            }
        };

        var mappingOptions = new CategoryToListMappingOptions
        {
            CategoryToList = new Dictionary<string, string>(),
            DefaultCategory = "Default"
        };

        var logger = new LoggerStub<GoogleSheetWrapper>();
        var wrapper = new GoogleSheetWrapper(googleService, options, mappingOptions, logger);

        var income = new MoneyTransfer
        {
            Date = new DateTime(2024, 12, 25),
            Category = "Salary",
            Description = "End of year bonus",
            Amount = 1000,
            Currency = Currency.USD
        };

        // Act
        await wrapper.SaveIncome(income, CancellationToken.None);

        // Assert
        Assert.Equal("Incomes!A1:J1", googleService.LastUpdatedRange);
        var row = googleService.LastUpdatedValues![0];
        Assert.Equal("=YEAR(C1)", row[0]);
        Assert.Equal("=MONTH(C1)", row[1]);
        Assert.Equal("25.12.2024", row[2]);
        Assert.Equal("Salary", row[3]);
        Assert.Equal("End of year bonus", row[4]);
        Assert.Equal(1000, (decimal) row[8]);
    }
}