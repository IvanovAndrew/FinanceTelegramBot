using Domain;
using Xunit;

namespace Application.Test.BotTransitionsTest;

public class AddExpensesFromYerevanCityTest
{
    [Fact]
    public async Task AddOutcomeByManualRequisites()
    {
        var scenario = await BotScenario.Start();
        
        // Arrange
        scenario.YerevanCityReceiptProvider.Responses[(new DateOnly(2023, 6, 20), "0123456789")] = 
            new List<Outcome>()
            {
                new ()
                {
                    Date = new DateOnly(2023, 06, 20),
                    Category = Categories.Outcome.Food,
                    SubCategory = Categories.Outcome.Food.GetSubcategoryByName("Snacks"),
                    Shop = Shop.Create("Yerevan city"),
                    Description = "Lays",
                    Amount = new Money{Currency = Currency.AMD, Amount = 500m},
                },
                new()
                {
                    Date = new DateOnly(2023, 06, 20),
                    Category = Categories.Outcome.Food,
                    SubCategory = Categories.Outcome.Food.GetSubcategoryByName("Products"),
                    Shop = Shop.Create("Yerevan city"),
                    Description = "Marianna",
                    Amount = new Money{Currency = Currency.AMD, Amount = 500.64m},
                },
            };
        
        // Act
        await scenario.ChooseOutcome();
        await scenario.ChooseCheckByYerevanCityRequisites();
        await scenario.WithCustomDate("20.06.2023");
        await scenario.WithCheckId("0123456789");

        // Assert
        Assert.Contains("Categories: Еда", scenario.LastMessage.Text);
        Assert.Contains("Subcategories", scenario.LastMessage.Text);
        Assert.Contains("Перекусы, Продукты", scenario.LastMessage.Text);
        Assert.Contains("saved with", scenario.LastMessage.Text);
    }
}