using Application.Contracts;
using Domain;
using Domain.Check;
using Xunit;

namespace Application.Test.BotTransitionsTest;

public class AddExpensesFromFNSTest
{
    [Fact]
    public async Task AddOutcomeByManualRequisites()
    {
        var scenario = await BotScenario.Start();
        
        var checkRequisite = new CheckRequisite()
        {
            DateTime = new DateTime(2023, 6, 20),
            TotalPrice = 1000.64m,
            FiscalNumber = FiscalNumber.Create("1234567890123456").Value,
            FiscalDocumentNumber = FiscalDocumentNumber.Create("1234").Value,
            FiscalDocumentSign = FiscalDocumentSign.Create("1234567").Value,
        };

        await scenario.Repo.SaveAllOutcomes(
            [
                new Outcome()
                {
                    Amount = new Money() { Amount = 1, Currency = Currency.RUR }, Category = Categories.Outcome.Food,
                    SubCategory = Categories.Outcome.Food.Sub("Snacks"), Description = "Lays"
                },
                new Outcome()
                {
                    Amount = new Money() { Amount = 10, Currency = Currency.RUR }, Category = Categories.Outcome.Food,
                    SubCategory = Categories.Outcome.Food.Sub("Products"), Description = "Marianna"
                }
            ], default
        );
        
        // Arrange
        scenario.FnsService.Responses[checkRequisite] = new List<RawOutcomeItem>()
        {
            new ()
            {
                Date = new DateOnly(2023, 06, 20),
                Description = "Lays",
                Amount = 500m,
            },
            new()
            {
                Date = new DateOnly(2023, 06, 20),
                Description = "Marianna",
                Amount = 500.64m,
            },
        };
        
        // Act
        await scenario.ChooseOutcome();
        await scenario.ChooseCheckByRequisites();
        await scenario.WithDate("20.06.2023");
        await scenario.WithPrice("1000.64");
        await scenario.WithFiscalNumber("1234567890123456");
        await scenario.WithFiscalDocument("1234");
        await scenario.WithFiscalDocumentSign("1234567");

        // Assert
        Assert.Contains("Categories: Еда", scenario.LastMessage.Text);
        Assert.Contains("Subcategories", scenario.LastMessage.Text);
        Assert.Contains("Перекусы, Продукты", scenario.LastMessage.Text);
        Assert.Contains("saved with", scenario.LastMessage.Text);
    }
}