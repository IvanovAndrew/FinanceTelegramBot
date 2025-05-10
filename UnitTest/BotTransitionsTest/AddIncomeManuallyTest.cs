using Domain;
using Infrastructure;
using UnitTest.Extensions;
using Xunit;

namespace UnitTest.BotTransitionsTest;

public class AddIncomeManuallyTest
{
    [Fact]
    public async Task ClickOnSaveButtonSavesTheIncome()
    {
        // Arrange
        var telegramBot = new MessageServiceMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2024, 9, 14));
        var outcomeCategories = new Category[]
        {
            new CategoryBuilder("Food").WithSubcategory("Snacks").WithSubcategory("Products").Build(),
            new CategoryBuilder("Cats").Build(),
        };

        var incomeCategories = new Category[]
        {
            new CategoryBuilder("Other").Build(),
        };
        
        var finanseRepository = new FinanceRepositoryStub();
        var userSession = new UserSessionService();
        var botEngine = BotEngineWrapper.Create(outcomeCategories, incomeCategories, finanseRepository, dateTimeService, telegramBot, userSession);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("income");
        await botEngine.Proceed("Another day");
        await botEngine.Proceed("08.09.2024");
        await botEngine.Proceed("Other");
        await botEngine.Proceed("Improvisation class");
        await botEngine.Proceed("8000 amd");
        var lastMessage = await botEngine.Proceed("Save");

        var savedIncomes = await finanseRepository.ReadIncomes(new FinanceFilter(), default);
        var savedIncome = savedIncomes.First();
        
        // Assert
        Assert.EndsWith("Saved", lastMessage.Text);
        Assert.Equal(new DateOnly(2024, 9, 8), savedIncome.Date);
        Assert.Equal("Other", savedIncome.Category);
        Assert.Equal("Improvisation class", savedIncome.Description);
        Assert.Equal(new Money(){Amount = 8_000, Currency = Currency.Amd}, savedIncome.Amount);
    }
}