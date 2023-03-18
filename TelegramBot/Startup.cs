using System;
using System.Globalization;
using Domain;
using GoogleSheet;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using TelegramBot.Controllers;
using TelegramBot.Services;

namespace TelegramBot;

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
        services.AddSingleton<IDateParser, DateParser>(s =>
            ActivatorUtilities.CreateInstance<DateParser>(s, new CultureInfo("en-US")));
        services.AddSingleton<TelegramBotService>(s => ActivatorUtilities.CreateInstance<TelegramBotService>(s, _configuration.GetSection("Telegram")["Url"], _configuration.GetSection("Telegram")["Token"]));
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
                AmountAmdColumn = _configuration["SpreadsheetOptions:Lists:Usual:Columns:AmountAMD"] 
            };

            instance.FlatInfo = new ListInfo
            {
                ListName = _configuration["SpreadsheetOptions:Lists:Home:Name"],
                YearColumn = _configuration["SpreadsheetOptions:Lists:Home:Columns:Year"],
                MonthColumn = _configuration["SpreadsheetOptions:Lists:Home:Columns:Month"],
                DateColumn = _configuration["SpreadsheetOptions:Lists:Home:Columns:Date"],
                DescriptionColumn = _configuration["SpreadsheetOptions:Lists:Home:Columns:Description"],
                AmountRurColumn = _configuration["SpreadsheetOptions:Lists:Home:Columns:AmountRUR"],
                AmountAmdColumn = _configuration["SpreadsheetOptions:Lists:Home:Columns:AmountAMD"],
            };

            instance.BigDealInfo = new ListInfo()
            {
                ListName = _configuration["SpreadsheetOptions:Lists:BigDeal:Name"],
                YearColumn = _configuration["SpreadsheetOptions:Lists:BigDeal:Columns:Year"],
                MonthColumn = _configuration["SpreadsheetOptions:Lists:BigDeal:Columns:Month"],
                DateColumn = _configuration["SpreadsheetOptions:Lists:BigDeal:Columns:Date"],
                DescriptionColumn = _configuration["SpreadsheetOptions:Lists:BigDeal:Columns:Description"],
                AmountRurColumn = _configuration["SpreadsheetOptions:Lists:BigDeal:Columns:AmountRUR"],
                AmountAmdColumn = _configuration["SpreadsheetOptions:Lists:BigDeal:Columns:AmountAMD"],
            };

            return instance;
        });
        services.AddSingleton<GoogleSheetWrapper>(s => ActivatorUtilities.CreateInstance<GoogleSheetWrapper>(s, s.GetRequiredService<SheetOptions>(), _configuration["SpreadsheetOptions:ApplicationName"], _configuration["SpreadsheetOptions:SpreadsheetID"]));

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
        serviceProvider.GetRequiredService<TelegramBotService>().GetBot().Wait();
        
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    } 
}