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


        services.AddSingleton<SheetOptions>(s =>
        {
            var instance = ActivatorUtilities.CreateInstance<SheetOptions>(s) as SheetOptions;
            instance.EveryDayExpenses = new ListInfo()
            {
                IsIncome = false,
                ListName = "Повседневные",
                
                YearToFirstExcelRow = new Dictionary<int, int>()
                {
                    [2022] = 1,
                    [2023] = 1700, 
                    [2024] = 5400 
                },
                
                YearColumn = "A",
                MonthColumn = "B",
                DateColumn = "C",
                CategoryColumn = "D",
                SubCategoryColumn = "E",
                DescriptionColumn = "F",
                AmountRurColumn = "G",
                AmountAmdColumn = "H",
                AmountGelColumn = "I"
            };
            instance.FlatInfo = new ListInfo()
            {
                IsIncome = false,
                ListName = "Квартира",
                Category = "Квартира",
                YearColumn = "A",
                MonthColumn = "B",
                DateColumn = "C",
                SubCategoryColumn = "D",
                DescriptionColumn = "E",
                AmountRurColumn = "F",
                AmountAmdColumn = "G",
                AmountGelColumn = "H"
            };
            instance.BigDealInfo = new ListInfo()
            {
                IsIncome = false,
                ListName = "Крупные",
                DateColumn = "C",
                CategoryColumn = "D",
                DescriptionColumn = "E",
                AmountRurColumn = "F",
                AmountAmdColumn = "G",
                AmountGelColumn = "H",
            };
            instance.CurrencyConversion = new ListInfo()
            {
                IsIncome = false,
                ListName = "Обмен валюты",
                Category = "Обмен валюты",
                DateColumn = "C",
                AmountRurColumn = "H",
                AmountAmdColumn = "I",
                AmountGelColumn = "J"
            };
            instance.CurrencyConversionIncome = new ListInfo()
            {
                IsIncome = true,
                ListName = "Обмен валюты",
                Category = "Обмен валюты",
                DateColumn = "C",
                AmountRurColumn = "L",
                AmountAmdColumn = "M",
                AmountGelColumn = "P"
            };
            instance.Incomes = new ListInfo()
            {
                IsIncome = true,
                ListName = "Доходы",
                YearColumn = "A",
                MonthColumn = "B",
                DateColumn = "C",
                CategoryColumn = "D",
                DescriptionColumn = "E",
                AmountRurColumn = "F",
                AmountAmdColumn = "G",
                AmountGelColumn = "H"
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
            ActivatorUtilities.CreateInstance<GoogleSheetWrapper>(s, s.GetRequiredService<SheetOptions>(), s.GetRequiredService<CategoryToListMappingOptions>(), applicationName, spreadsheetId, s.GetRequiredService<ILogger<GoogleSheetWrapper>>()));
        services.AddScoped<GoogleSheetAzureFunction>();
        services.AddScoped<AzureQueueTrigger>();
    })
    .Build();

host.Run();