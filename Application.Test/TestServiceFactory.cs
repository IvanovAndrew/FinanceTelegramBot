using Application.Contracts;
using Application.Events;
using Application.Test.Extensions;
using Application.Test.Stubs;
using Domain;
using Infrastructure;
using Infrastructure.Fns;
using Microsoft.Extensions.DependencyInjection;
using UnitTest;
using UnitTest.Stubs;

namespace Application.Test;

public static class TestServiceFactory
{
    public static IServiceProvider Create(
        out FinanceRepositoryStub financeRepository,
        out DateTimeServiceStub dateTimeService,
        out MessageServiceMock messageService, 
        out FnsServiceStub fnsService)
    {
        var services = new ServiceCollection();

        financeRepository = new FinanceRepositoryStub();
        dateTimeService = new DateTimeServiceStub(new DateTime(2023, 6, 29));
        messageService = new MessageServiceMock();
        fnsService = new FnsServiceStub();
        var userSession = new UserSessionService();
        

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UserStartedEventHandler).Assembly));
        services.AddSingleton<IFinanceRepository>(financeRepository);
        services.AddSingleton<IFnsService>(fnsService);
        services.AddSingleton<IDateTimeService>(dateTimeService);
        services.AddSingleton<IMessageService>(messageService);
        services.AddSingleton<IUserSessionService>(userSession);
        services.AddSingleton<IExpenseJsonParser, ExpenseJsonParser>();
        services.AddSingleton<ICategoryProvider, CategoryProviderStub>();
        services.AddSingleton<IExpenseCategorizer, ExpenseHistoryCategorizer>();
        services.AddSingleton(provider => BotEngineWrapper.Create(
            provider.GetRequiredService<ICategoryProvider>(),
            (FinanceRepositoryStub)provider.GetRequiredService<IFinanceRepository>(),
            (DateTimeServiceStub)provider.GetRequiredService<IDateTimeService>(),
            (MessageServiceMock)provider.GetRequiredService<IMessageService>(),
            provider.GetRequiredService<IUserSessionService>(),
            provider.GetRequiredService<IFnsService>()
        ));


        services.AddLogging();

        return services.BuildServiceProvider();
    }
}