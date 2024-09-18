using Domain;
using NUnit.Framework;

namespace UnitTest;

public class AggregatorTest
{
    [Test]
    public void CategorySum()
    {
        var expenseAggregator = new ExpensesAggregator<string>(e => e.Category, true, false);

        var currencies = new[] { Currency.Amd, Currency.Rur };
        var expenses = new List<Outcome>()
        {
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), Category = "Cats",
                Amount = new Money() { Amount = 5_000m, Currency = Currency.Amd }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Cats",
                Amount = new Money() { Amount = 10_000m, Currency = Currency.Amd }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Cats",
                Amount = new Money() { Amount = 1_000m, Currency = Currency.Rur }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Food",
                Amount = new Money() { Amount = 300m, Currency = Currency.Rur }
            },
        };
        var statistics = expenseAggregator.Aggregate(expenses, currencies);

        var catsExpenses = statistics.Rows.First(c => c.Row == "Cats");
        Assert.That(catsExpenses[Currency.Amd], Is.EqualTo(new Money(){Amount = 15_000m, Currency = Currency.Amd}));
        Assert.That(catsExpenses[Currency.Rur], Is.EqualTo(new Money(){Amount = 1_000m, Currency = Currency.Rur}));
        
        Assert.That(statistics.Total[Currency.Amd], Is.EqualTo(new Money(){Amount = 15_000m, Currency = Currency.Amd}));
        Assert.That(statistics.Total[Currency.Rur], Is.EqualTo(new Money(){Amount = 1_300m, Currency = Currency.Rur}));
    }
    
    [Test]
    public void TotalsCalculation()
    {
        var expenseAggregator = new ExpensesAggregator<string>(e => e.Category, true, false);

        var currencies = new[] { Currency.Amd, Currency.Rur };
        var expenses = new List<Outcome>()
        {
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), Category = "Cats",
                Amount = new Money() { Amount = 5_000m, Currency = Currency.Amd }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Cats",
                Amount = new Money() { Amount = 10_000m, Currency = Currency.Amd }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Cats",
                Amount = new Money() { Amount = 1_000m, Currency = Currency.Rur }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Food",
                Amount = new Money() { Amount = 300m, Currency = Currency.Rur }
            },
        };
        var statistics = expenseAggregator.Aggregate(expenses, currencies);

        Assert.That(statistics.Total[Currency.Amd], Is.EqualTo(new Money(){Amount = 15_000m, Currency = Currency.Amd}));
        Assert.That(statistics.Total[Currency.Rur], Is.EqualTo(new Money(){Amount = 1_300m, Currency = Currency.Rur}));
    }
    
    [Test]
    public void UnrequestedCurrencyIsIgnored()
    {
        var expenseAggregator = new ExpensesAggregator<string>(e => e.Category, true, false);

        var currencies = new[] { Currency.Amd};
        var expenses = new List<Outcome>()
        {
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), Category = "Cats",
                Amount = new Money() { Amount = 5_000m, Currency = Currency.Amd }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Cats",
                Amount = new Money() { Amount = 10_000m, Currency = Currency.Amd }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Cats",
                Amount = new Money() { Amount = 1_000m, Currency = Currency.Rur }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Food",
                Amount = new Money() { Amount = 300m, Currency = Currency.Rur }
            },
        };
        
        // Act
        var statistics = expenseAggregator.Aggregate(expenses, currencies);

        // Assert
        var catsExpenses = statistics.Rows.First(c => c.Row == "Cats");
        Assert.That(catsExpenses[Currency.Amd], Is.EqualTo(new Money(){Amount = 15_000m, Currency = Currency.Amd}));
    }
    
    [Test]
    public void WhenExpensesInTheCurrencyAreMissingThenZeroWillBeReturned()
    {
        var expenseAggregator = new ExpensesAggregator<string>(e => e.Category, true, false);

        var currencies = new[] { Currency.Amd, Currency.Rur, };
        var expenses = new List<Outcome>()
        {
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), Category = "Cats",
                Amount = new Money() { Amount = 5_000m, Currency = Currency.Amd }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Cats",
                Amount = new Money() { Amount = 10_000m, Currency = Currency.Amd }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Cats",
                Amount = new Money() { Amount = 1_000m, Currency = Currency.Rur }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Food",
                Amount = new Money() { Amount = 300m, Currency = Currency.Rur }
            },
        };
        
        // Act
        var statistics = expenseAggregator.Aggregate(expenses, currencies);

        // Assert
        var foodExpenses = statistics.Rows.First(c => c.Row == "Food");
        Assert.That(foodExpenses[Currency.Amd], Is.EqualTo(new Money(){Amount = 0m, Currency = Currency.Amd}));
        Assert.That(foodExpenses[Currency.Rur], Is.EqualTo(new Money(){Amount = 300m, Currency = Currency.Rur}));
    }
    
    
    [Test]
    public void DateSorting()
    {
        var expenseAggregator = new ExpensesAggregator<DateOnly>(e => e.Date, false, true);

        var currencies = new[] { Currency.Amd, Currency.Rur };
        var expenses = new List<Outcome>()
        {
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), Category = "Cats",
                Amount = new Money() { Amount = 5_000m, Currency = Currency.Amd }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Cats",
                Amount = new Money() { Amount = 10_000m, Currency = Currency.Amd }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(-2)), Category = "Cats",
                Amount = new Money() { Amount = 1_000m, Currency = Currency.Rur }
            },
            new()
            {
                Date = DateOnly.FromDateTime(DateTime.Today), Category = "Food",
                Amount = new Money() { Amount = 300m, Currency = Currency.Rur }
            },
        };
        // Act
        var statistics = expenseAggregator.Aggregate(expenses, currencies);

        // Assert
        CollectionAssert.AreEqual(
            new []
            {
                DateOnly.FromDateTime(DateTime.Today.AddDays(-2)),
                DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
                DateOnly.FromDateTime(DateTime.Today),
            }, statistics.Rows.Select(row => row.Row));
        
    }
}