using Application;
using Application.Contracts;
using Application.Events;
using Domain;
using Infrastructure;
using Infrastructure.Fns;
using Infrastructure.GoogleSpreadsheet;
using Infrastructure.Telegram;
using MediatR;
using Microsoft.OpenApi.Models;
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
            var builder = services.AddControllers().AddNewtonsoftJson();
            services.ConfigureTelegramBotMvc();

            services.AddLogging();
            services.AddSingleton<IDateTimeService, DateTimeService>();
            services.AddSingleton<IExpenseJsonParser, ExpenseJsonParser>();
            services.AddSingleton<IUserSessionService, UserSessionService>();
            services.AddSingleton<IMessageService, TelegramMessageService>();

            var telegramToken = Environment.GetEnvironmentVariable("TELEGRAM_TOKEN");

            if (!long.TryParse(Environment.GetEnvironmentVariable("TELEGRAM_SUPPORT_CHAT"), out var supportChatId))
            {
                supportChatId = 0;
            }
            
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UserStartedEventHandler).Assembly));
            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(SavingExpenseNotificationBehavior<,>));
            
            services.AddRefitClient<IFnsApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://proverkacheka.com"));
            services.AddSingleton<IFnsAPIService, FnsApiService>(s =>
                ActivatorUtilities.CreateInstance<FnsApiService>(s, Environment.GetEnvironmentVariable("FNS_TOKEN")));
            services.AddSingleton<ICategoryProvider, CategoryProvider>();
            services.AddSingleton<ITelegramBotClient, TelegramBotClient>(s => ActivatorUtilities.CreateInstance<TelegramBotClient>(s, telegramToken));
        
            services.AddTransient<RefitMessageHandler>();
            
            services.AddRefitClient<IGoogleSpreadsheetApi>(new RefitSettings
                {
                    ContentSerializer = new NewtonsoftJsonContentSerializer()
                })
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri(Environment.GetEnvironmentVariable("GOOGLESPREADSHEET_URL"));
                })
                .AddHttpMessageHandler<RefitMessageHandler>();

            services.AddScoped<IGoogleSpreadsheetService, GoogleSpreadsheetService>();
            
            // Register the core service
            services.AddScoped<FinanceRepository>();

            // Register the decorator by specifying it to use the core service as a dependency
            services.AddScoped<IFinanceRepository>(provider =>
            {
                var coreService = provider.GetRequiredService<FinanceRepository>();
                var categoryProvider = provider.GetRequiredService<ICategoryProvider>();
                var logger = provider.GetRequiredService<ILogger<FinanceRepositoryDecorator>>();
                return new FinanceRepositoryDecorator(coreService, categoryProvider, logger);
            });

            services.AddScoped<IExpenseCategorizer, ExpenseHistoryCategorizer>();
            
            services.AddSwaggerGen(c =>
                c.SwaggerDoc("v1", new OpenApiInfo() { Title = "Warrior's finance bot", Version = "v1" }));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Warrior's finance bot"));
        
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        } 
    }
}