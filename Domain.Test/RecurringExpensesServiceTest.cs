using Domain.Services;

namespace Domain.Test;

public class RecurringExpensesServiceTest
{
    [Fact]
    public void RecurringExpenseHappened()
    {
        var definition = new RecurringExpenseDefinition(
            "Abonement", 
            Categories.Outcome.Hobby, 
            Categories.Outcome.Hobby.GetSubcategoryByName("Sport"), 
            Shop.Create("RAU Pool"), 
            RecurringFrequency.Monthly, 
            Way.Fixed, new Money { Amount = 25_000, Currency = Currency.AMD});
        
        var expenses = new List<Outcome>()
        {
            new Outcome()
            {
                Date = new DateOnly(2026, 9, 3),
                Category = Categories.Outcome.Hobby,
                SubCategory = Categories.Outcome.Hobby.GetSubcategoryByName("Sport"),
                Shop = Shop.Create("RAU Pool"),
                Amount = new Money { Amount = 25_000, Currency = Currency.AMD}
            }
        };
        
        var today = new DateOnly(2026, 9, 5);

        var result = Execute(new List<RecurringExpenseDefinition> { definition }, expenses, today);
        
        Assert.Empty(result);
    }
    
    [Fact]
    public void WeeklyExpense_AlreadyPaidThisWeek_StillForecastsNextWeekBeforeMonthEnd()
    {
        var today = new DateOnly(2026, 9, 15);
        
        var definition = new RecurringExpenseDefinition(
            "Psycologist",
            Categories.Outcome.Psycologist,
            null,
            null,
            RecurringFrequency.Weekly,
            Way.Fixed,
            new Money {Amount = 3000m, Currency = Currency.RUR}
        );

        var history = new List<Outcome>
        {
            new()
            {
                Category = definition.Category,
                SubCategory = definition.SubCategory,
                Shop = definition.Shop,
                Date = today, 
                Amount = new Money {Amount = 3000m, Currency = Currency.RUR},
            }
        };

        var service = new RecurringExpensesService();

        var result = service.GetMissingRecurringExpenses([definition], history, today);

        Assert.NotEmpty(result);
        Assert.All(result, m => Assert.Equal(definition, m.Definition));
        Assert.All(result, m => Assert.Equal(new Money {Amount = 3000m, Currency = Currency.RUR}, m.ResolvedAmount));

        Assert.Contains(result, _ => true);
    }

    [Fact]
    public void WeeklyExpense_NoPaymentYet_CurrentWeekIsReportedAsMissing()
    {
        var today = new DateOnly(2026, 9, 15);
        
        var definition = new RecurringExpenseDefinition(
            "Psycologist",
            Categories.Outcome.Psycologist,
            null,
            null,
            RecurringFrequency.Weekly,
            Way.Fixed,
            new Money {Amount = 3000m, Currency = Currency.RUR}
        );

        var service = new RecurringExpensesService();

        var result = service.GetMissingRecurringExpenses([definition], Enumerable.Empty<Outcome>(), today);

        Assert.NotEmpty(result);
    }

    private IReadOnlyList<MissingRecurringExpense> Execute(IReadOnlyCollection<RecurringExpenseDefinition> definitions,
        IEnumerable<Outcome> allExpenses,
        DateOnly today)
    {
        return (new RecurringExpensesService()).GetMissingRecurringExpenses(definitions, allExpenses, today);
    }
}