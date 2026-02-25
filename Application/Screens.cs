using Domain;

namespace Application;

public enum ScreenMode
{
    Update,
    Replace,
    Ignore
}

public class Screen
{
    public string Text { get; init; } = string.Empty;
    public IReadOnlyList<Option> Options { get; init; } = [];
    public ScreenMode Mode { get; init; }
    public byte[]? Bytes { get; init; }
    public Table? Table { get; init; }
}

public static class Screens
{
    public static Screen EnterTheSource() =>
        new Screen()
        {
            Text = "Enter the way",
            Options = [new Option("/myself", "By myself"), new Option("/check", "From check")],
            Mode = ScreenMode.Update
        };

    public static Screen EnterDateTime(string? text = null) =>
        new Screen()
        {
            Text = text ?? "Enter the check date and time",
            Mode = ScreenMode.Replace,
        };
    
    public static Screen SelectDay(DateOnly today) =>
        new Screen()
        {
            Text = "Enter the date",
            Options = [
                new Option(today.ToString(DateFormat.DayOnlyNumbers), "Today"),
                new Option(today.AddDays(-1).ToString(DateFormat.DayOnlyNumbers), "Yesterday"),
                new Option("Another day")],
            Mode = ScreenMode.Update
        };
    
    public static Screen SelectCustomDay(DateOnly today) =>
        new Screen()
        {
            Text = $"Enter the month. Example: {today.ToString(DateFormat.DayOnlyNumbers)}",
            Mode = ScreenMode.Replace
        };
    
    public static Screen SelectMonth(YearMonth current) =>
        new Screen()
        {
            Text = "Enter the month",
            Options = [
                new Option(current.ToString(DateFormat.FullMonthName)), 
                new Option(current.Previous().ToString(DateFormat.FullMonthName)), 
                new Option("Another month")],
            Mode = ScreenMode.Update
        };
    
    public static Screen SelectCustomMonth(YearMonth current) =>
        new Screen()
        {
            Text = $"Enter the month. Example: {current.ToString(DateFormat.FullMonthName)}",
            Mode = ScreenMode.Replace
        };

    public static Screen SelectCategory(IEnumerable<Category> categories) =>
        new Screen()
        {
            Text = "Enter the category",
            Options = categories.Select(c => new Option(c.Code, c.ShortName ?? c.Name)).ToList(),
            Mode = ScreenMode.Update
        };
    
    public static Screen SelectSubCategory(IEnumerable<SubCategory> subCategories) =>
        new Screen()
        {
            Text = "Enter the subcategory",
            Options = subCategories.Select(c => new Option(c.Code, c.ShortName ?? c.Name)).ToList(),
            Mode = ScreenMode.Update
        };

    public static Screen EnterDescription() =>
        new Screen()
        {
            Text = "Enter the description",
            Mode = ScreenMode.Replace,
        };
    
    public static Screen EnterPrice() =>
        new Screen()
        {
            Text = "Enter the price",
            Mode = ScreenMode.Replace,
        };
    
    public static Screen Confirm(string text) =>
        new Screen()
        {
            Text = text,
            Options = [new Option("/save", "Save"), new Option("/cancel", "Cancel")],
            Mode = ScreenMode.Replace,
        };

    public static Screen SelectCurrency(IEnumerable<Currency> currencies) =>
        new Screen()
        {
            Text = "Enter the currency",
            Options = currencies.Select(c => new Option(c.Name)).ToList(),
            Mode = ScreenMode.Update,
        };
    
    public static Screen EnterFiscalNumber() =>
        new Screen()
        {
            Text = "Enter the fiscal number. It should contain 16 digits",
        };

    public static Screen EnterFiscalDocumentNumber() => new Screen()
    {
        Text = "Enter the document number"
    };

    public static Screen EnterFiscalDocumentSign() =>
        new Screen()
        {
            Text = "Enter the fiscal document sign. It should contain only digit",
        };

    public static Screen EnterText(string text) => 
        new Screen(){Text = text, Mode = ScreenMode.Replace};

    public static Screen SelectCheckSource() => 
        new Screen()
        {
            Text = "Enter the check",
            Options = [new Option("/json", "json"), new Option("/url", "Url Link"), new Option("/requisites", "By Requisites")],
            Mode =  ScreenMode.Update
        };

    public static Screen Notify(string text, byte[]? bytes = null) =>
        new Screen()
        {
            Text = text,
            Bytes = bytes,
            Mode = ScreenMode.Ignore,
        };

    public static Screen SelectStart() => 
        new Screen()
        {
            Text = "What would you like to do?",
            Options = [new Option("/outcome", "Outcome"), new Option("/income", "Income"), new Option("/statistics", "Statistics")],
            Mode = ScreenMode.Update
        };

    public static Screen SelectStatistic() => new Screen()
    {
        Text = "Select statistic",
        Options = [
            new Option ("/balance", "Balance"),
            new Option ("/statisticByDay", "Day expenses (by categories)"),
            new Option ("/statisticByMonth", "Month expenses (by categories)"), 
            new Option ("/statisticByCategory", "Category expenses (by months)"), 
            new Option ("/statisticBySubcategory", "Subcategory expenses (overall)"), 
            new Option ("/statisticBySubcategoryByMonth", "Subcategory expenses (by months)")
        ],
    };

    public static Screen NotifyLoading(string text) =>
        new Screen()
        {
            Text = text,
            Mode = ScreenMode.Update,
        };

    public static Screen Notify(Table table) =>
        new Screen()
        {
            Table = table,
            Mode = ScreenMode.Ignore,
        };

}