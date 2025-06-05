using Application;
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
            services.AddSingleton<IFnsService, FnsService>(s =>
                ActivatorUtilities.CreateInstance<FnsService>(s, Environment.GetEnvironmentVariable("FNS_TOKEN")));
            services.AddSingleton<ICategoryProvider, CategoryProvider>();
            services.AddSingleton<ITelegramBotClient, TelegramBotClient>(s => ActivatorUtilities.CreateInstance<TelegramBotClient>(s, telegramToken));
        
            services.AddTransient<LoggingHandler>();
            
            services.AddRefitClient<IGoogleSpreadsheetApi>(new RefitSettings
                {
                    ContentSerializer = new NewtonsoftJsonContentSerializer()
                })
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri(Environment.GetEnvironmentVariable("GOOGLESPREADSHEET_URL"));
                })
                .AddHttpMessageHandler<LoggingHandler>();

            services.AddScoped<IGoogleSpreadsheetService, GoogleSpreadsheetService>();
            
            // Register the core service
            services.AddScoped<FinanceRepository>();

            // Register the decorator by specifying it to use the core service as a dependency
            services.AddScoped<IFinanceRepository>(provider =>
            {
                var coreService = provider.GetRequiredService<FinanceRepository>();
                var logger = provider.GetRequiredService<ILogger<FinanceRepositoryDecorator>>();
                return new FinanceRepositoryDecorator(coreService, logger);
            });
            
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
    
    public class LoggingHandler : DelegatingHandler
    {
        private readonly ILogger<LoggingHandler> _logger;

        public LoggingHandler(ILogger<LoggingHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string? body = null;
            if (request.Content != null)
            {
                body = await request.Content.ReadAsStringAsync(cancellationToken);
            }

            _logger.LogInformation("[HttpClient] Request to {Uri}\nMethod: {Method}\nBody:\n{Body}", request.RequestUri, request.Method, body ?? "<null>");

            return await base.SendAsync(request, cancellationToken);
        }
    }


}