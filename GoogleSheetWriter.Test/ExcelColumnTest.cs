namespace GoogleSheetWriter.Tests;

public class ExcelColumnTests
{
    [Theory]
    [InlineData("A", 1)]
    [InlineData("Z", 26)]
    [InlineData("AA", 27)]
    [InlineData("AZ", 52)]
    public void ToNumber_should_return_expected_numeric_value_for_column_name(string name, int expectedNumber)
    {
        var column = ExcelColumn.FromString(name);
        Assert.Equal(expectedNumber, column.ToNumber());
    }

    [Theory]
    [InlineData(1, "A")]
    [InlineData(26, "Z")]
    [InlineData(27, "AA")]
    [InlineData(52, "AZ")]
    [InlineData(53, "BA")]
    [InlineData(702, "ZZ")]
    [InlineData(703, "AAA")]
    public void FromNumber_should_return_expected_column_name_for_number(int number, string expectedName)
    {
        var column = ExcelColumn.FromNumber(number);
        Assert.Equal(expectedName, column.Name);
    }

    [Fact]
    public void Columns_with_same_letters_in_different_case_should_be_equal()
    {
        var column1 = ExcelColumn.FromString("aa");
        var column2 = ExcelColumn.FromString("AA");
        Assert.Equal(column1, column2);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("1A")]
    [InlineData("A!")]
    public void FromString_should_throw_if_column_name_is_invalid(string name)
    {
        Assert.Throws<ArgumentException>(() => ExcelColumn.FromString(name));
    }

    [Fact]
    public void FromNumber_should_throw_if_number_is_less_than_one()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ExcelColumn.FromNumber(0));
    }

    [Fact]
    public void DifferenceBetween_should_return_difference_between_two_columns()
    {
        var column1 = ExcelColumn.FromString("A");
        var column2 = ExcelColumn.FromString("D");
        Assert.Equal(-3, ExcelColumn.DifferenceBetween(column1, column2));
    }

    [Fact]
    public void ColumnsBetween_should_return_all_columns_in_range_if_order_is_ascending()
    {
        var result = ExcelColumn.ColumnsBetween(ExcelColumn.FromString("A"), ExcelColumn.FromString("C"));
        Assert.Equal(new[] { "A", "B", "C" }, Array.ConvertAll(result, c => c.Name));
    }

    [Fact]
    public void ColumnsBetween_should_return_all_columns_in_range_if_order_is_descending()
    {
        var result = ExcelColumn.ColumnsBetween(ExcelColumn.FromString("C"), ExcelColumn.FromString("A"));
        Assert.Equal(new[] { "A", "B", "C" }, Array.ConvertAll(result, c => c.Name));
    }

    [Fact]
    public void CompareTo_should_return_correct_ordering_based_on_column_position()
    {
        var columnA = ExcelColumn.FromString("A");
        var columnB = ExcelColumn.FromString("B");
        Assert.True(columnA < columnB);
        Assert.True(columnB > columnA);
        Assert.True(columnA <= columnB);
        Assert.True(columnB >= columnA);
    }
}
