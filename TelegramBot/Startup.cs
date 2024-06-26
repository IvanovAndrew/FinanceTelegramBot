using GoogleSheetWriter;
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
        
            services.AddScoped<ICurrencyParser, CurrencyParser>();
            services.AddScoped<IMoneyParser, MoneyParser>(s => ActivatorUtilities.CreateInstance<MoneyParser>(s, s.GetRequiredService<ICurrencyParser>()));

            services.AddSingleton<SheetOptions>(s =>
            {
                var instance = ActivatorUtilities.CreateInstance<SheetOptions>(s) as SheetOptions;
                instance.UsualExpenses = new ListInfo
                {
                    ListName = _configuration["SpreadsheetOptions:Lists:Usual:Name"],
                    YearColumn = _configuration["SpreadsheetOptions:Lists:Usual:Columns:Year"],
                    MonthColumn = _configuration["SpreadsheetOptions:Lists:Usual:Columns:Month"],
                    DateColumn = _configuration["SpreadsheetOptions:Lists:Usual:Columns:Date"],
                    CategoryColumn = _configuration["SpreadsheetOptions:Lists:Usual:Columns:Category"],
                    SubCategoryColumn = _configuration["SpreadsheetOptions:Lists:Usual:Columns:SubCategory"],
                    DescriptionColumn = _configuration["SpreadsheetOptions:Lists:Usual:Columns:Description"],
                    AmountRurColumn = _configuration["SpreadsheetOptions:Lists:Usual:Columns:AmountRUR"],
                    AmountAmdColumn = _configuration["SpreadsheetOptions:Lists:Usual:Columns:AmountAMD"], 
                    AmountGelColumn = _configuration["SpreadsheetOptions:Lists:Usual:Columns:AmountGEL"] 
                };

                instance.FlatInfo = new ListInfo
                {
                    ListName = _configuration["SpreadsheetOptions:Lists:Home:Name"],
                    Category = _configuration["SpreadsheetOptions:Lists:Home:Name"],
                    YearColumn = _configuration["SpreadsheetOptions:Lists:Home:Columns:Year"],
                    MonthColumn = _configuration["SpreadsheetOptions:Lists:Home:Columns:Month"],
                    DateColumn = _configuration["SpreadsheetOptions:Lists:Home:Columns:Date"],
                    SubCategoryColumn = _configuration["SpreadsheetOptions:Lists:Home:Columns:SubCategory"],
                    DescriptionColumn = _configuration["SpreadsheetOptions:Lists:Home:Columns:Description"],
                    AmountRurColumn = _configuration["SpreadsheetOptions:Lists:Home:Columns:AmountRUR"],
                    AmountAmdColumn = _configuration["SpreadsheetOptions:Lists:Home:Columns:AmountAMD"],
                    AmountGelColumn = _configuration["SpreadsheetOptions:Lists:Home:Columns:AmountGEL"],
                };

                instance.BigDealInfo = new ListInfo()
                {
                    ListName = _configuration["SpreadsheetOptions:Lists:BigDeal:Name"],
                    Category = _configuration["SpreadsheetOptions:Lists:BigDeal:Name"],
                    YearColumn = _configuration["SpreadsheetOptions:Lists:BigDeal:Columns:Year"],
                    MonthColumn = _configuration["SpreadsheetOptions:Lists:BigDeal:Columns:Month"],
                    DateColumn = _configuration["SpreadsheetOptions:Lists:BigDeal:Columns:Date"],
                    SubCategoryColumn = _configuration["SpreadsheetOptions:Lists:Home:Columns:SubCategory"],
                    DescriptionColumn = _configuration["SpreadsheetOptions:Lists:BigDeal:Columns:Description"],
                    AmountRurColumn = _configuration["SpreadsheetOptions:Lists:BigDeal:Columns:AmountRUR"],
                    AmountAmdColumn = _configuration["SpreadsheetOptions:Lists:BigDeal:Columns:AmountAMD"],
                    AmountGelColumn = _configuration["SpreadsheetOptions:Lists:BigDeal:Columns:AmountGEL"],
                };

                return instance;
            });
            
            services.AddSingleton<CategoryToListMappingOptions>(s =>
            {
                var instance = ActivatorUtilities.CreateInstance<CategoryToListMappingOptions>(s);
                instance.DefaultCategory = _configuration["CategoryToList:DefaultList"];
                instance.CategoryToList = new Dictionary<string, string>();
                foreach (var child in _configuration.GetSection("CategoryToList:CustomMapping").GetChildren())
                {
                    instance.CategoryToList[child["Category"]] = child["List"];
                }

                return instance;
            });

            services.AddSingleton<GoogleSheetWrapper>(s => ActivatorUtilities.CreateInstance<GoogleSheetWrapper>(s, s.GetRequiredService<SheetOptions>(), s.GetRequiredService<CategoryToListMappingOptions>(), _configuration["SpreadsheetOptions:ApplicationName"], _configuration["SpreadsheetOptions:SpreadsheetID"]));
            
            services.AddSingleton<IExpenseRepository, ExpenseRepositoryDecorator>(s => 
                ActivatorUtilities.CreateInstance<ExpenseRepositoryDecorator>(s, s.GetRequiredService<GoogleSheetWrapper>()));
            
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