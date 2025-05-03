using Domain;
using Infrastructure;
using NUnit.Framework;
using UnitTest.Extensions;
using UnitTest.Stubs;

namespace UnitTest.BotTransitionsTest;

public class AddExpensesFromFNSTest
{
    [Test]
    public async Task AddOutcomeByManualRequisites()
    {
        // Arrange
        var telegramBot = new MessageServiceMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new FinanceRepositoryStub();
        var userSession = new UserSessionService();
        var fnsService = new FnsServiceStub
        {
            Responses =
            {
                ["t=20230620T0000&s=1000.64&fn=1234567890123456&i=1234&fp=1234567&n=1"] = new List<Outcome>()
                {
                    new Outcome
                    {
                        Category = "Food", SubCategory = "Snacks", Date = new DateOnly(2023, 06, 20), Amount = new Money(){Amount = 500m, Currency = Currency.Rur},
                    },
                    new Outcome
                    {
                        Category = "Food", SubCategory = "Products", Date = new DateOnly(2023, 06, 20), Amount = new Money(){Amount = 500.64m, Currency = Currency.Rur},
                    },
                }
            }
        };
        var botEngine = BotEngineWrapper.Create(categories, [], expenseRepository, dateTimeService, telegramBot, userSession, fnsService);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("outcome");
        await botEngine.Proceed("From Check");
        await botEngine.Proceed("By Requisites");
        await botEngine.Proceed("20.06.2023");
        await botEngine.Proceed("1000.64");
        await botEngine.Proceed("1234567890123456");
        await botEngine.Proceed("1234");
        var lastMessage = await botEngine.Proceed("1234567");

        // Assert
        StringAssert.Contains("Category: Food", lastMessage.Text);
        StringAssert.Contains("saved with", lastMessage.Text);
    }
}