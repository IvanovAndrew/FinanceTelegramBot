using GoogleSheetWriter;
using GoogleSpreadsheet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddLogging();

        string spreadsheetId = Environment.GetEnvironmentVariable("SpreadsheetID")?? string.Empty;
        string applicationName = Environment.GetEnvironmentVariable("ApplicationName")?? string.Empty;

        services.AddSingleton<IGoogleService, GoogleService>(s =>
            ActivatorUtilities.CreateInstance<GoogleService>(s, applicationName, spreadsheetId,
                s.GetRequiredService<ILogger<GoogleService>>()));

        services.AddSingleton<SheetOptions>(s =>
        {
            var instance = ActivatorUtilities.CreateInstance<SheetOptions>(s) as SheetOptions;
            instance.EveryDayExpenses = new ListInfo()
            {
                ListName = "Повседневные",
                
                YearToFirstExcelRow = new Dictionary<int, int>()
                {
                    [2022] = 1,
                    [2023] = 1700, 
                    [2024] = 5400,
                    [2025] = 10800,
                },
                
                YearColumn = ExcelColumn.FromString("A"),
                MonthColumn = ExcelColumn.FromString("B"),
                DateColumn = ExcelColumn.FromString("C"),
                CategoryColumn = ExcelColumn.FromString("D"),
                SubCategoryColumn = ExcelColumn.FromString("E"),
                DescriptionColumn = ExcelColumn.FromString("F"),
                AmountRurColumn = ExcelColumn.FromString("G"),
                AmountAmdColumn = ExcelColumn.FromString("H"),
                AmountGelColumn = ExcelColumn.FromString("I"),
                AmountUsdColumn = ExcelColumn.FromString("J"),
            };
            instance.FlatInfo = new ListInfo()
            {
                ListName = "Квартира",
                Category = "Квартира",
                YearColumn = ExcelColumn.FromString("A"),
                MonthColumn = ExcelColumn.FromString("B"),
                DateColumn = ExcelColumn.FromString("C"),
                CategoryColumn = ExcelColumn.FromString("D"),
                DescriptionColumn = ExcelColumn.FromString("E"),
                AmountRurColumn = ExcelColumn.FromString("F"),
                AmountAmdColumn = ExcelColumn.FromString("G"),
                AmountGelColumn = ExcelColumn.FromString("H"),
                AmountUsdColumn = ExcelColumn.FromString("I"),
            };
            instance.BigDealInfo = new ListInfo()
            {
                ListName = "Крупные",
                DateColumn = ExcelColumn.FromString("C"),
                CategoryColumn = ExcelColumn.FromString("D"),
                DescriptionColumn = ExcelColumn.FromString("E"),
                AmountRurColumn = ExcelColumn.FromString("F"),
                AmountAmdColumn = ExcelColumn.FromString("G"),
                AmountGelColumn = ExcelColumn.FromString("H"),
                AmountUsdColumn = ExcelColumn.FromString("I"),
            };
            instance.CurrencyConversion = new ListInfo()
            {
                ListName = "Обмен валюты",
                Category = "Обмен валюты",
                DateColumn = ExcelColumn.FromString("C"),
                AmountRurColumn = ExcelColumn.FromString("H"),
                AmountAmdColumn = ExcelColumn.FromString("I"),
                AmountGelColumn = ExcelColumn.FromString("J"),
            };
            instance.CurrencyConversionIncome = new ListInfo()
            {
                IsIncome = true,
                ListName = "Обмен валюты",
                Category = "Обмен валюты",
                DateColumn = ExcelColumn.FromString("C"),
                AmountRurColumn = ExcelColumn.FromString("L"),
                AmountAmdColumn = ExcelColumn.FromString("M"),
                AmountUsdColumn = ExcelColumn.FromString("O"),
                AmountGelColumn = ExcelColumn.FromString("P")
            };
            instance.Incomes = new ListInfo()
            {
                IsIncome = true,
                ListName = "Доходы",
                YearColumn = ExcelColumn.FromString("A"),
                MonthColumn = ExcelColumn.FromString("B"),
                DateColumn = ExcelColumn.FromString("C"),
                CategoryColumn = ExcelColumn.FromString("D"),
                DescriptionColumn = ExcelColumn.FromString("E"),
                AmountRurColumn = ExcelColumn.FromString("F"),
                AmountAmdColumn = ExcelColumn.FromString("G"),
                AmountGelColumn = ExcelColumn.FromString("H"),
                AmountUsdColumn = ExcelColumn.FromString("I"),
            };
            return instance;
        });

        services.AddSingleton<CategoryToListMappingOptions>(s =>
            {
                var instance = ActivatorUtilities.CreateInstance<CategoryToListMappingOptions>(s);

                instance.DefaultCategory = "Повседневные";
                instance.CategoryToList = new Dictionary<string, string>()
                {
                    ["Квартира"] = "Квартира",
                    ["Обмен валюты"] = "Обмен валюты",
                    ["Перелёты"] = "Крупные",
                    ["Операция"] = "Крупные",
                    ["Документы"] = "Крупные",
                    ["Отель"] = "Крупные",
                    ["Техника"] = "Крупные",
                };

                return instance;
            }
        );

        services.AddSingleton<GoogleSheetWrapper>(s =>
            ActivatorUtilities.CreateInstance<GoogleSheetWrapper>(s, s.GetRequiredService<IGoogleService>(), s.GetRequiredService<SheetOptions>(), s.GetRequiredService<CategoryToListMappingOptions>(), s.GetRequiredService<ILogger<GoogleSheetWrapper>>()));
        services.AddScoped<GoogleSheetAzureFunction>();
        services.AddScoped<AzureQueueTrigger>();
    })
    .Build();

host.Run();