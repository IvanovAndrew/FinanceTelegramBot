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

    private IReadOnlyList<MissingRecurringExpense> Execute(IReadOnlyCollection<RecurringExpenseDefinition> definitions,
        IEnumerable<Outcome> allExpenses,
        DateOnly today)
    {
        return (new RecurringExpensesService()).GetMissingRecurringExpenses(definitions, allExpenses, today);
    }
}