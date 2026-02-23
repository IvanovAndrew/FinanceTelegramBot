using Application.Contracts;
using Application.Events;
using Application.Services;
using Application.Test.Extensions;
using Application.Test.Stubs;
using Domain;
using Domain.Services;
using Infrastructure;
using Infrastructure.Fns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Test;

public static class TestServiceFactory
{
    public static IServiceProvider Create(
        out FinanceRepositoryStub financeRepository,
        out DateTimeServiceStub dateTimeService,
        out MessageServiceMock messageService, 
        out FnsApiServiceStub fnsApiService, 
        out SalaryDayServiceStub salaryDayService)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMemoryCache();
        
        services.AddSingleton<ILoggerFactory, LoggerFactory>();
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        financeRepository = new FinanceRepositoryStub();
        dateTimeService = new DateTimeServiceStub(new DateTime(2023, 6, 29));
        messageService = new MessageServiceMock();
        fnsApiService = new FnsApiServiceStub();
        salaryDayService = new SalaryDayServiceStub();
        var userSession = new UserSessionService();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UserStartedEventHandler).Assembly));
        services.AddSingleton<IFinanceRepository>(financeRepository);
        services.AddSingleton<IFnsAPIService>(fnsApiService);
        services.AddSingleton<IDateTimeService>(dateTimeService);
        services.AddSingleton<MessageServiceMock>(messageService);
        services.AddSingleton<IMessageService>(messageService);
        services.AddSingleton<IUserSessionService>(userSession);
        services.AddSingleton<IExpenseJsonParser, RussianCheckExpenseJsonParser>();
        services.AddSingleton<IRecurringExpensesService, RecurringExpensesService>();
        
        services.AddSingleton<ICurrencyProvider, CurrencyProviderStub>();
        services.AddSingleton<IExpenseCategorizer, ExpenseHistoryCategorizer>();
        services.AddSingleton<ICheckDownloader, CheckDownloader>();
        services.AddSingleton<IBalanceStatisticService, BalanceStatisticService>();
        services.AddSingleton<FinanceStatisticsService>();
        services.AddSingleton<ISalaryScheduleProvider, SalaryScheduleProvider>();
        services.AddSingleton<ISalaryDayService>(salaryDayService);
        services.AddSingleton<ISpendingDayPolicy, SpendingDayPolicy>();
        
        services.AddSingleton<IExpenseCategoryMappingCache, ExpenseCategoryMappingCache>();
        services.AddSingleton<IPictureGenerator, PictureGeneratorStub>();
        
        services.AddSingleton<FlowOrchestrator>();
        
        services.AddSingleton<BotEngine>();
        services.AddSingleton<BotEngineWrapper>();

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true,
        });
    }
}