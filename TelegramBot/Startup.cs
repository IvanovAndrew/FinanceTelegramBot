using Infrastructure;
using Microsoft.OpenApi.Models;
using StateMachine;
using TelegramBot.Services;

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

            services.AddLogging();
            services.AddSingleton<IDateTimeService, DateTimeService>();
            services.AddSingleton<TelegramBotService>(s => ActivatorUtilities.CreateInstance<TelegramBotService>(s, 
                _configuration.GetSection("Telegram")["Token"], 
                long.Parse(_configuration.GetSection("Telegram")["SupportChatId"])));
            services.AddSingleton<IFnsService, FnsService>(s =>
                ActivatorUtilities.CreateInstance<FnsService>(s, _configuration.GetSection("Fns")["Token"]));
            services.AddSingleton<CategoryOptions>();
        
            
            services.AddScoped<IMoneyParser, MoneyParser>(s => ActivatorUtilities.CreateInstance<MoneyParser>(s, s.GetRequiredService<ICurrencyParser>()));
            services.AddScoped<ICurrencyParser, CurrencyParser>();

            services.AddSingleton<IGoogleSpreadsheetService, GoogleSpreadsheetService>(s =>
                ActivatorUtilities.CreateInstance<GoogleSpreadsheetService>(s, _configuration.GetSection("GoogleSpreadsheet")["Url"]));

            // Register the core service
            services.AddScoped<ExpenseRepository>();

            // Register the decorator by specifying it to use the core service as a dependency
            services.AddScoped<IExpenseRepository>(provider =>
            {
                var coreService = provider.GetRequiredService<ExpenseRepository>();
                var logger = provider.GetRequiredService<ILogger<ExpenseRepositoryDecorator>>();
                return new ExpenseRepositoryDecorator(coreService, logger);
            });
            
            services.AddScoped<IStateFactory, StateFactory>(s => ActivatorUtilities.CreateInstance<StateFactory>(s,
                s.GetRequiredService<IDateTimeService>(),
                s.GetRequiredService<IMoneyParser>(),
                s.GetRequiredService<CategoryOptions>().Categories,
                s.GetRequiredService<IExpenseRepository>()));
            
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