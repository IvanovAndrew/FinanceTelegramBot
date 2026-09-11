using Application.Bot;
using Application.Bot.Flows;
using Application.Core;
using Application.Core.Services;
using Application.Test.Extensions;
using Application.Test.Stubs;
using Domain;
using Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Test;

public static class TestServiceFactory
{
    public static (IServiceProvider Provider, BotTestEnvironment Environment) Create()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var environment = new BotTestEnvironment
        {
            Repo = new FinanceRepositoryStub(),
            Time = new DateTimeServiceStub(new DateTime(2023, 6, 29)),
            MessageService = new MessageServiceMock(),
            FnsReceiptProvider = new FnsReceiptProviderStub(),
            YerevanCityReceiptProvider = new YerevanCityReceiptProviderStub(),
            SalaryDayService = new SalaryDayServiceStub(),
            ExpenseJsonParser = new ExpenseJsonParserStub(),
        };

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(LongOperationCanceledEvent).Assembly));

        services.AddSingleton<IFinanceRepository>(environment.Repo);
        services.AddSingleton<ICurrencyExchangeOutcomeMatcher, CurrencyExchangeOutcomeMatcher>();
        services.AddSingleton<IYerevanCityReceiptProvider>(environment.YerevanCityReceiptProvider);
        services.AddSingleton<IUserRepository, UserRepositoryStub>();
        services.AddSingleton<IFlowStepRenderer, FlowStepRenderer>();
        services.AddSingleton<IDateTimeService>(environment.Time);
        services.AddSingleton<MessageServiceMock>(environment.MessageService);
        services.AddSingleton<IMessageService>(environment.MessageService);
        services.AddSingleton<IExpensesService, ExpensesService>();
        services.AddSingleton<IUserSessionService, UserSessionServiceStub>();
        services.AddSingleton<IConversation, ConversationStub>();
        services.AddSingleton<IExpenseCategorizer, ExpenseCategorizerStub>();
        services.AddSingleton<IExpenseCategoryMappingCache, ExpenseCategoryMappingCacheStub>();
        
        services.AddSingleton<IExpenseJsonParser>(environment.ExpenseJsonParser);

        services.AddSingleton<IRecurringExpensesService, RecurringExpensesService>();
        services.AddSingleton<IRecurringExpenseDefinitionsRepository, RecurringExpenseDefinitionsRepositoryStub>();

        services.AddSingleton<IFnsReceiptProvider>(environment.FnsReceiptProvider);
        services.AddSingleton<IExternalCategoryMapper, ExternalCategoryMapper>();
        services.AddSingleton<IBalanceStatisticService, BalanceStatisticService>();
        services.AddSingleton<FinanceStatisticsService>();
        services.AddSingleton<ISalaryScheduleProvider, SalaryScheduleProvider>();
        services.AddSingleton<ISalaryDayService>(environment.SalaryDayService);
        services.AddSingleton<ISpendingDayPolicy, SpendingDayPolicy>();

        services.AddSingleton<IPictureGenerator, PictureGeneratorStub>();

        services.AddSingleton<FlowOrchestrator>();

        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<IAdminNotificationService, AdminNotificationService>();
        services.AddSingleton<BotEngine>();
        services.AddSingleton<BotEngineWrapper>();

        var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true,
        });

        return (provider, environment);
    }
}