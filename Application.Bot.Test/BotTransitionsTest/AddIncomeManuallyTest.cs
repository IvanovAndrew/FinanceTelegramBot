using Domain;
using Xunit;

namespace Application.Test.BotTransitionsTest;

public class AddIncomeManuallyTest
{
    [Fact]
    public async Task ClickOnSaveButtonSavesTheIncome()
    {
        // Act
        var scenario = await BotScenario.Start();
        await scenario.ChooseIncome();
        await scenario.WithCustomDate("08.09.2024");
        await scenario.WithCategory("Прочее");
        await scenario.WithDescription("Improvisation class");
        await scenario.WithPrice("8000 amd");
        await scenario.ConfirmSaving();

        var savedIncomes = await scenario.Repo.ReadIncomes(new FinanceFilter(), default);
        var savedIncome = savedIncomes.First();
        
        // Assert
        Assert.EndsWith("Saved", scenario.LastMessage.Text);
        Assert.Equal(new DateOnly(2024, 9, 8), savedIncome.Date);
        Assert.Equal(Categories.Income.Others, savedIncome.Category);
        Assert.Equal("Improvisation class", savedIncome.Description);
        Assert.Equal(new Money(){Amount = 8_000, Currency = Currency.AMD}, savedIncome.Amount);
    }
}