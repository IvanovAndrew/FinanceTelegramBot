using Application.Contracts;
using Application.Test.Extensions;
using Application.Test.Stubs;
using Domain;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Application.Test.BotTransitionsTest;

public class AddExpensesFromFNSTest
{
    private readonly BotEngineWrapper _botEngine;
    private readonly FinanceRepositoryStub _expenseRepository;
    private readonly FnsServiceStub _fnsService;

    public AddExpensesFromFNSTest()
    {
        var provider = TestServiceFactory.Create(out _expenseRepository, out _, out _, out _fnsService);

        _botEngine = provider.GetRequiredService<BotEngineWrapper>();
    }
    
    [Fact]
    public async Task AddOutcomeByManualRequisites()
    {
        var checkRequisite = new CheckRequisite()
        {
            DateTime = new DateTime(2023, 6, 20),
            TotalPrice = 1000.64m,
            FiscalNumber = "1234567890123456",
            FiscalDocumentNumber = "1234",
            FiscalDocumentSign = "1234567"
        };

        await _expenseRepository.SaveAllOutcomes(
            new[]
            {
                new Outcome()
                {
                    Amount = new Money() { Amount = 1, Currency = Currency.RUR }, Category = "Food".AsCategory(),
                    SubCategory = "Snacks".AsSubcategory(), Description = "Lays"
                },
                new Outcome()
                {
                    Amount = new Money() { Amount = 10, Currency = Currency.RUR }, Category = "Food".AsCategory(),
                    SubCategory = "Products".AsSubcategory(), Description = "Marianna"
                },
            }, new CancellationToken()
        );
        
        // Arrange
        _fnsService.Responses[checkRequisite] = new List<RawOutcomeItem>()
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
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("outcome");
        await _botEngine.Proceed("From Check");
        await _botEngine.Proceed("By Requisites");
        await _botEngine.Proceed("20.06.2023");
        await _botEngine.Proceed("1000.64");
        await _botEngine.Proceed("1234567890123456");
        await _botEngine.Proceed("1234");
        var lastMessage = await _botEngine.Proceed("1234567");

        // Assert
        Assert.Contains("Categories: Food", lastMessage.Text);
        Assert.Contains("Subcategories", lastMessage.Text);
        Assert.Contains("Snacks, Products", lastMessage.Text);
        Assert.Contains("saved with", lastMessage.Text);
    }
}