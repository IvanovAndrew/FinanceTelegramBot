using Domain;
using Infrastructure;
using NUnit.Framework;
using UnitTest.Extensions;

namespace UnitTest.BotTransitionsTest;

public class AddIncomeManuallyTest
{
    [Test]
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
        StringAssert.EndsWith("Saved", lastMessage.Text);
        Assert.That(() => new DateOnly(2024, 9, 8) == savedIncome.Date);
        Assert.That(savedIncome.Category, Is.EqualTo("Other"));
        Assert.That(savedIncome.Description, Is.EqualTo("Improvisation class"));
        Assert.That(savedIncome.Amount, Is.EqualTo(new Money(){Amount = 8_000, Currency = Currency.Amd}));
    }
}