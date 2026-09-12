using System.Globalization;
using Application.Api;
using Application.Bot;
using Application.Contracts;
using Application.Core;
using Application.Core.Services;
using Domain;
using Domain.Services;
using Infrastructure;
using Infrastructure.Fns;
using Infrastructure.GoogleSpreadsheet;
using Infrastructure.HealthChecks;
using Infrastructure.Telegram;
using Infrastructure.YerevanCity;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi;
using Polly;
using Refit;
using Telegram.Bot;

namespace TelegramBot
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var builder = services.AddControllers();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy
                        .AllowAnyOrigin() // Или .WithOrigins("http://localhost:3000", "https://your-telegram-app.com")
                        .AllowAnyHeader() // Разрешает ваш заголовок Authorization: tma ...
                        .AllowAnyMethod(); // Разрешает GET, POST, OPTIONS и т.д.
                });
            });

            services.AddLogging();
            services.AddSingleton<IDateTimeService, DateTimeService>();
            services.AddSingleton<IUserSessionService, UserSessionService>();
            services.AddSingleton<IMessageService, TelegramMessageService>();
            services.AddSingleton<IConversation, TelegramConversation>();
            services.AddSingleton<IConversationStateStore, ConversationStateStore>();

            services.AddMemoryCache();
            services.AddScoped<IExpenseCategoryMappingCache, ExpenseCategoryMappingCache>();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(LongOperationCanceledEvent).Assembly, typeof(GetBalanceStatisticApiCommand).Assembly, typeof(GetDayOutcomesRequestCommandHandler).Assembly));

            services.AddRefitClient<IFnsApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://proverkacheka.com"));
            services.AddSingleton<IFnsAPIService, FnsApiService>(s =>
                ActivatorUtilities.CreateInstance<FnsApiService>(s, s.GetRequiredService<IFnsApi>(),
                    Environment.GetEnvironmentVariable("FNS_TOKEN") ?? "FNS_TOKEN"));
            services.AddSingleton<IRecurringExpensesService, RecurringExpensesService>();
            services.AddTransient<FinanceStatisticsService>();
            
            var telegramToken = Environment.GetEnvironmentVariable("TELEGRAM_TOKEN");
            builder.Services.AddHttpClient("telegram_bot")
                .AddTypedClient<ITelegramBotClient>((httpClient) => 
                    new TelegramBotClient(telegramToken, httpClient));

            services.AddSingleton<IFnsReceiptProvider, FnsReceiptProvider>();

            services.Configure<SalarySettings>(_configuration.GetSection("SalarySettings"));
            services.AddSingleton<ISalaryDayService, SalaryDayService>();
            services.AddSingleton<ISalaryScheduleProvider, SalaryScheduleProvider>();
            services.AddSingleton<ISpendingDayPolicy, SpendingDayPolicy>();
            services.AddScoped<IBalanceStatisticService, BalanceStatisticService>();

            services.AddTransient<RefitMessageHandler>();

            services.AddRefitClient<IGoogleSpreadsheetApi>()
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri(Environment.GetEnvironmentVariable("GOOGLESPREADSHEET_URL"));
                    c.Timeout = TimeSpan.FromMinutes(2);
                })
                .AddHttpMessageHandler<RefitMessageHandler>()
                .AddTransientHttpErrorPolicy(policy => policy
                    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, retryAttempt))))
                .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
                {
                    PooledConnectionLifetime = TimeSpan.FromMinutes(2),
                    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1)
                });

            services.AddSingleton<IGoogleSpreadsheetService, GoogleSpreadsheetService>();
            services.AddSingleton<IRecurringExpenseDefinitionsRepository, RecurringExpenseDefinitionsRepository>();

            services.AddSingleton<IPictureGenerator, ScottPlotPictureGenerator>();

            // Register the core service
            services.AddSingleton<FinanceRepository>();

            // Register the decorator by specifying it to use the core service as a dependency
            services.AddSingleton<IFinanceRepository>(provider =>
            {
                var coreService = provider.GetRequiredService<FinanceRepository>();
                var logger = provider.GetRequiredService<ILogger<FinanceRepositoryDecorator>>();
                return new FinanceRepositoryDecorator(coreService, logger);
            });

            services.AddScoped<IExpenseCategorizer, ExpenseHistoryCategorizer>();
            services.AddSingleton<IExternalCategoryMapper, ExternalCategoryMapper>();
            services.AddSingleton<ICurrencyExchangeOutcomeMatcher, CurrencyExchangeOutcomeMatcher>();
            services.AddSingleton<IFlowStepRenderer, FlowStepRenderer>();
            
            services.AddSingleton<IExpensesService, ExpensesService>();

            services.AddSingleton<YerevanCityExpenseJsonParser>();
            services.AddSingleton<RussianCheckExpenseJsonParser>();
            services.AddSingleton<IFnsShopNameResolver, FnsShopNameResolver>();

            services.AddSingleton<IYerevanCityReceiptProvider, YerevanCityReceiptProvider>();
            builder.Services.AddHttpClient<IYerevanCityAPI, YerevanCityAPI>((httpClient, _) => new YerevanCityAPI(httpClient, Environment.GetEnvironmentVariable("YEREVANCITY_AUTH")));
            
            services.AddSingleton<IExpenseJsonParser>(sp => new ExpenseJsonParserChain([
                sp.GetRequiredService<YerevanCityExpenseJsonParser>(),
                sp.GetRequiredService<RussianCheckExpenseJsonParser>()
            ]));
            
            builder.Services.AddHealthChecks()
                .AddCheck<GoogleSpreadsheetHealthCheck>("GoogleSpreadsheet")
                .AddCheck<YerevanCityHealthCheck>("Yerevan city API")
                .AddCheck<FnsHealthCheck>("FNS API");

            var authenticatedIds = ParseIdsFromEnv("AUTHENTICATED_USER_IDS");
            var adminIds = ParseIdsFromEnv("ADMIN_USER_IDS");

            services.AddSingleton(new UserSettings
            {
                Authenticated = authenticatedIds,
                Admins = adminIds
            });

            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IAdminNotificationService, AdminNotificationService>();
            services.AddScoped<BotEngine>();

            services.AddSwaggerGen(c =>
                c.SwaggerDoc("v1", new OpenApiInfo() { Title = "Warrior's finance bot", Version = "v1" }));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Warrior's finance bot"));
            }

            app.UseCors("AllowAll");
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                
                endpoints.MapHealthChecks("/api/health", new HealthCheckOptions
                {
                    ResponseWriter = async (context, report) =>
                    {
                        context.Response.ContentType = "application/json";

                        var response = report.Entries.ToDictionary(
                            entry => entry.Key,
                            entry => entry.Value.Status == HealthStatus.Healthy
                        );

                        await context.Response.WriteAsJsonAsync(response);
                    }
                });
            });
        }

        private static List<long> ParseIdsFromEnv(string variableName) =>
            (Environment.GetEnvironmentVariable(variableName) ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(long.Parse)
            .ToList();
    }
}