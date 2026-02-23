using Application.Test.Extensions;
using Application.Test.Stubs;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Application.Test.BotTransitionsTest;

public class AddExpenseFromJson
{
    private readonly BotEngineWrapper _botEngine;
    private readonly FinanceRepositoryStub _expenseRepository;
    private readonly FnsApiServiceStub _fnsApiService;

    public AddExpenseFromJson()
    {
        var provider = TestServiceFactory.Create(out _expenseRepository, out _, out _, out _fnsApiService, out _);

        _botEngine = provider.GetRequiredService<BotEngineWrapper>();
    }
    
    [Fact]
    public async Task AddJson()
    {
        // Arrange
        
        
        // Act
        await _botEngine.Proceed("/start");
        await _botEngine.Proceed("outcome");
        await _botEngine.Proceed("From Check");
        var lastMessage = await _botEngine.Proceed("json");

        // Assert
        Assert.Contains("Paste", lastMessage.Text);
        Assert.Contains("a json file", lastMessage.Text);
        Assert.Null(lastMessage.Options);
    }
}