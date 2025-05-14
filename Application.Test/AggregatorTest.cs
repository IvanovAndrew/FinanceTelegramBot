using Application.Test.Extensions;
using Domain;
using Xunit;

namespace Application.Test;

public class AggregatorTest
{
    [Fact]
    public void CategorySum()
    {
        var expenseAggregator = new ExpensesAggregator<string>(e => e.Category.Name, true, false);

        var currencies = new[] { Currency.Amd, Currency.Rur };
        var expenses = new List<Outcome>()
        {
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 5_000m, Currency = Currency.Amd }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 10_000m, Currency = Currency.Amd }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 1_000m, Currency = Currency.Rur }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Food".AsCategory(),
                Amount = new Money() { Amount = 300m, Currency = Currency.Rur }
            },
        };
        var statistics = expenseAggregator.Aggregate(expenses, currencies);

        var catsExpenses = statistics.Rows.First(c => c.Row == "Cats");
        Assert.Equal(new Money(){Amount = 15_000m, Currency = Currency.Amd}, catsExpenses[Currency.Amd]);
        Assert.Equal(new Money(){Amount = 1_000m, Currency = Currency.Rur}, catsExpenses[Currency.Rur]);
        
        Assert.Equal(new Money(){Amount = 15_000m, Currency = Currency.Amd}, statistics.Total[Currency.Amd]);
        Assert.Equal(new Money(){Amount = 1_300m, Currency = Currency.Rur}, statistics.Total[Currency.Rur]);
    }
    
    [Fact]
    public void TotalsCalculation()
    {
        var expenseAggregator = new ExpensesAggregator<string>(e => e.Category.Name, true, false);

        var currencies = new[] { Currency.Amd, Currency.Rur };
        var expenses = new List<Outcome>()
        {
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 5_000m, Currency = Currency.Amd }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 10_000m, Currency = Currency.Amd }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 1_000m, Currency = Currency.Rur }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 300m, Currency = Currency.Rur }
            },
        };
        var statistics = expenseAggregator.Aggregate(expenses, currencies);

        Assert.Equal(new Money(){Amount = 15_000m, Currency = Currency.Amd}, statistics.Total[Currency.Amd]);
        Assert.Equal(new Money(){Amount = 1_300m, Currency = Currency.Rur}, statistics.Total[Currency.Rur]);
    }
    
    [Fact]
    public void UnrequestedCurrencyIsIgnored()
    {
        var expenseAggregator = new ExpensesAggregator<string>(e => e.Category.Name, true, false);

        var currencies = new[] { Currency.Amd};
        var expenses = new List<Outcome>()
        {
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 5_000m, Currency = Currency.Amd }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 10_000m, Currency = Currency.Amd }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 1_000m, Currency = Currency.Rur }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Food".AsCategory(),
                Amount = new Money() { Amount = 300m, Currency = Currency.Rur }
            },
        };
        
        // Act
        var statistics = expenseAggregator.Aggregate(expenses, currencies);

        // Assert
        var catsExpenses = statistics.Rows.First(c => c.Row == "Cats");
        Assert.Equal(new Money(){Amount = 15_000m, Currency = Currency.Amd}, catsExpenses[Currency.Amd]);
    }
    
    [Fact]
    public void WhenExpensesInTheCurrencyAreMissingThenZeroWillBeReturned()
    {
        var expenseAggregator = new ExpensesAggregator<string>(e => e.Category.Name, true, false);

        var currencies = new[] { Currency.Amd, Currency.Rur, };
        var expenses = new List<Outcome>()
        {
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 5_000m, Currency = Currency.Amd }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 10_000m, Currency = Currency.Amd }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 1_000m, Currency = Currency.Rur }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Food".AsCategory(),
                Amount = new Money() { Amount = 300m, Currency = Currency.Rur }
            },
        };
        
        // Act
        var statistics = expenseAggregator.Aggregate(expenses, currencies);

        // Assert
        var foodExpenses = statistics.Rows.First(c => c.Row == "Food");
        Assert.Equal(new Money(){Amount = 0m, Currency = Currency.Amd}, foodExpenses[Currency.Amd]);
        Assert.Equal(new Money(){Amount = 300m, Currency = Currency.Rur}, foodExpenses[Currency.Rur]);
    }
    
    
    [Fact]
    public void DateSorting()
    {
        var expenseAggregator = new ExpensesAggregator<DateOnly>(e => e.Date, false, true);

        var currencies = new[] { Currency.Amd, Currency.Rur };
        var expenses = new List<Outcome>()
        {
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 5_000m, Currency = Currency.Amd }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 10_000m, Currency = Currency.Amd }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-2)), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 1_000m, Currency = Currency.Rur }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Food".AsCategory(),
                Amount = new Money() { Amount = 300m, Currency = Currency.Rur }
            },
        };
        // Act
        var statistics = expenseAggregator.Aggregate(expenses, currencies);

        // Assert
        Assert.Equivalent(
            new []
            {
                DateOnly.FromDateTime(DateTime.Today.AddDays(-2)),
                DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
                DateOnly.FromDateTime(DateTime.Today),
            }, statistics.Rows.Select(row => row.Row));
    }
}