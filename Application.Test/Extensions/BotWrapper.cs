using Application.Contracts;
using Application.Events;
using Application.Services;
using Application.Test.Stubs;
using Domain;
using Infrastructure;
using Infrastructure.Fns;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using UnitTest;
using UnitTest.Stubs;

namespace Application.Test.Extensions;

internal class BotEngineWrapper
{
    private readonly BotEngine _botEngine;
    private readonly MessageServiceMock _messageService;
    
    private BotEngineWrapper(BotEngine botEngine, MessageServiceMock messageService)
    {
        _botEngine = botEngine;
        _messageService = messageService;
    }

    internal async Task<IMessage> Proceed(string text)
    {
        var lastSendMessage = _messageService.SentMessages.LastOrDefault();

        var messageText = text;

        if (!messageText.StartsWith("/") && (lastSendMessage?.Options != null))
        {
            messageText = lastSendMessage.Options
                .AllOptions().FirstOrDefault(b => string.Equals(b.Text, text, StringComparison.InvariantCultureIgnoreCase))?.Code;

            if (messageText == null)
            {
                throw new InvalidOperationException( 
                    $"Couldn't find {text} option between {(string.Join(", ", lastSendMessage.Options.AllOptions().Select(o => o.Text)))}");
            }
        }
        
        await _botEngine.Proceed(new MessageStub() { Text = messageText }, default);
        
        var lastMessage = _messageService.SentMessages.OrderBy(m => m.Id).Last();
            
        return lastMessage;
    }
    
    internal async Task<IMessage> ProceedFile(FileInfoStub fileInfo)
    {
        var message = new MessageStub
        {
            FileInfo = fileInfo
        };

        await _botEngine.Proceed(message, default);

        var lastMessage = _messageService.SentMessages.OrderBy(m => m.Id).Last();
            
        return lastMessage;
    }
    
    internal static BotEngineWrapper Create(ICategoryProvider categoryProvider, FinanceRepositoryStub expenseRepository, DateTimeServiceStub dateTimeService, MessageServiceMock telegramBot, IUserSessionService userSessionService, IFnsService? fnsService)
    {
        var services = new ServiceCollection();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UserStartedEventHandler).Assembly));
        services.AddSingleton<IFinanceRepository>(expenseRepository);
        services.AddSingleton<IFnsService>(fnsService?? new FnsServiceStub());
        services.AddSingleton<IDateTimeService>(dateTimeService);
        services.AddSingleton<IMessageService>(telegramBot);
        services.AddSingleton<IUserSessionService>(userSessionService);
        services.AddSingleton<IExpenseJsonParser, ExpenseJsonParser>();
        services.AddSingleton<ICategoryProvider>(categoryProvider);
        services.AddSingleton<IExpenseCategorizer, ExpenseHistoryCategorizer>();
        services.AddLogging();

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        return new BotEngineWrapper(new BotEngine(mediator, userSessionService, new LoggerStub<BotEngine>()), telegramBot);
    }
}