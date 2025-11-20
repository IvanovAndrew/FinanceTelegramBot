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

        var currencies = new[] { Currency.AMD, Currency.RUR };
        var expenses = new List<Outcome>()
        {
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 5_000m, Currency = Currency.AMD }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 10_000m, Currency = Currency.AMD }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 1_000m, Currency = Currency.RUR }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Food".AsCategory(),
                Amount = new Money() { Amount = 300m, Currency = Currency.RUR }
            },
        };
        var statistics = expenseAggregator.Aggregate(expenses, currencies);

        var catsExpenses = statistics.Rows.First(c => c.Row == "Cats");
        Assert.Equal(new Money(){Amount = 15_000m, Currency = Currency.AMD}, catsExpenses[Currency.AMD]);
        Assert.Equal(new Money(){Amount = 1_000m, Currency = Currency.RUR}, catsExpenses[Currency.RUR]);
        
        Assert.Equal(new Money(){Amount = 15_000m, Currency = Currency.AMD}, statistics.Total[Currency.AMD]);
        Assert.Equal(new Money(){Amount = 1_300m, Currency = Currency.RUR}, statistics.Total[Currency.RUR]);
    }
    
    [Fact]
    public void TotalsCalculation()
    {
        var expenseAggregator = new ExpensesAggregator<string>(e => e.Category.Name, true, false);

        var currencies = new[] { Currency.AMD, Currency.RUR };
        var expenses = new List<Outcome>()
        {
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 5_000m, Currency = Currency.AMD }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 10_000m, Currency = Currency.AMD }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 1_000m, Currency = Currency.RUR }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 300m, Currency = Currency.RUR }
            },
        };
        var statistics = expenseAggregator.Aggregate(expenses, currencies);

        Assert.Equal(new Money(){Amount = 15_000m, Currency = Currency.AMD}, statistics.Total[Currency.AMD]);
        Assert.Equal(new Money(){Amount = 1_300m, Currency = Currency.RUR}, statistics.Total[Currency.RUR]);
    }
    
    [Fact]
    public void UnrequestedCurrencyIsIgnored()
    {
        var expenseAggregator = new ExpensesAggregator<string>(e => e.Category.Name, true, false);

        var currencies = new[] { Currency.AMD};
        var expenses = new List<Outcome>()
        {
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 5_000m, Currency = Currency.AMD }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 10_000m, Currency = Currency.AMD }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 1_000m, Currency = Currency.RUR }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Food".AsCategory(),
                Amount = new Money() { Amount = 300m, Currency = Currency.RUR }
            },
        };
        
        // Act
        var statistics = expenseAggregator.Aggregate(expenses, currencies);

        // Assert
        var catsExpenses = statistics.Rows.First(c => c.Row == "Cats");
        Assert.Equal(new Money(){Amount = 15_000m, Currency = Currency.AMD}, catsExpenses[Currency.AMD]);
    }
    
    [Fact]
    public void WhenExpensesInTheCurrencyAreMissingThenZeroWillBeReturned()
    {
        var expenseAggregator = new ExpensesAggregator<string>(e => e.Category.Name, true, false);

        var currencies = new[] { Currency.AMD, Currency.RUR, };
        var expenses = new List<Outcome>()
        {
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 5_000m, Currency = Currency.AMD }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 10_000m, Currency = Currency.AMD }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 1_000m, Currency = Currency.RUR }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Food".AsCategory(),
                Amount = new Money() { Amount = 300m, Currency = Currency.RUR }
            },
        };
        
        // Act
        var statistics = expenseAggregator.Aggregate(expenses, currencies);

        // Assert
        var foodExpenses = statistics.Rows.First(c => c.Row == "Food");
        Assert.Equal(new Money(){Amount = 0m, Currency = Currency.AMD}, foodExpenses[Currency.AMD]);
        Assert.Equal(new Money(){Amount = 300m, Currency = Currency.RUR}, foodExpenses[Currency.RUR]);
    }
    
    
    [Fact]
    public void DateSorting()
    {
        var expenseAggregator = new ExpensesAggregator<DateOnly>(e => e.Date, false, true);

        var currencies = new[] { Currency.AMD, Currency.RUR };
        var expenses = new List<Outcome>()
        {
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 5_000m, Currency = Currency.AMD }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 10_000m, Currency = Currency.AMD }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-2)), Category = "Cats".AsCategory(),
                Amount = new Money() { Amount = 1_000m, Currency = Currency.RUR }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Food".AsCategory(),
                Amount = new Money() { Amount = 300m, Currency = Currency.RUR }
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