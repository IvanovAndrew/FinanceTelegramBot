using Domain;
using GoogleSheet;
using Microsoft.OpenApi.Models;
using TelegramBot.Controllers;

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

        services.AddSingleton<Services.TelegramBot>(s => ActivatorUtilities.CreateInstance<Services.TelegramBot>(s, _configuration.GetSection("Telegram")["Url"], _configuration.GetSection("Telegram")["Token"]));
        services.AddSingleton<CategoryOptions>();
        
        services.AddScoped<ICurrencyParser, CurrencyParser>();
        services.AddScoped<IMoneyParser, MoneyParser>(s => ActivatorUtilities.CreateInstance<MoneyParser>(s, s.GetRequiredService<ICurrencyParser>()));

        services.AddSingleton<SheetOptions>(s =>
        {
            var instance = ActivatorUtilities.CreateInstance<SheetOptions>(s) as SheetOptions;
            var usualList = _configuration.GetSection("SpreadsheetOptions").GetSection("Lists").GetSection("Usual");
            var columns = usualList.GetSection("Columns");
            instance.ListName = usualList["Name"];
            instance.MonthColumn = _configuration["SpreadsheetOptions:Lists:Usual:Columns:Month"];
            instance.DateColumn = _configuration["SpreadsheetOptions:Lists:Usual:Columns:Date"];
            instance.CategoryColumn = _configuration["SpreadsheetOptions:Lists:Usual:Columns:Category"];
            instance.SubCategoryColumn = _configuration["SpreadsheetOptions:Lists:Usual:Columns:SubCategory"];
            instance.DescriptionColumn = _configuration["SpreadsheetOptions:Lists:Usual:Columns:Description"];
            instance.AmountRurColumn = _configuration["SpreadsheetOptions:Lists:Usual:Columns:AmountRUR"];
            instance.AmountAmdColumn = _configuration["SpreadsheetOptions:Lists:Usual:Columns:AmountAMD"];

            return instance;
        });
        services.AddSingleton<GoogleSheetWriter>(s => ActivatorUtilities.CreateInstance<GoogleSheetWriter>(s, s.GetRequiredService<SheetOptions>(), _configuration["SpreadsheetOptions:ApplicationName"], _configuration["SpreadsheetOptions:SpreadsheetID"]));

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
        serviceProvider.GetRequiredService<Services.TelegramBot>().GetBot().Wait();
        
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    } 
}