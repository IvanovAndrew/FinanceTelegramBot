using GoogleSheetWriter;
using GoogleSheetWriter.Abstractions;
using GoogleSheetWriter.Infrastructure;
using GoogleSpreadsheet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
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
                
                DateRowResolver = new DateRowResolver(new Dictionary<DateOnly, int>()
                {
                    [new DateOnly(2022, 1, 1)] = 1,
                    [new DateOnly(2023, 1, 1)] = 1925, 
                    [new DateOnly(2024, 1, 1)] = 5800,
                    [new DateOnly(2025, 1, 1)] = 11400,
                    [new DateOnly(2025, 2, 1)] = 11750,
                    [new DateOnly(2025, 3, 1)] = 12300,
                    [new DateOnly(2025, 4, 1)] = 13050,
                    [new DateOnly(2025, 5, 1)] = 13675,
                    [new DateOnly(2025, 6, 1)] = 14075,
                    [new DateOnly(2025, 7, 1)] = 14750,
                    [new DateOnly(2025, 8, 1)] = 15100,
                    [new DateOnly(2025, 9, 1)] = 15450,
                    [new DateOnly(2025, 10, 1)] = 16100,
                    [new DateOnly(2025, 11, 1)] = 16450,
                    [new DateOnly(2025, 12, 1)] = 16750,
                    [new DateOnly(2026, 1, 1)] = 17500,
                    [new DateOnly(2026, 2, 1)] = 18200,
                    [new DateOnly(2026, 3, 1)] = 18750,
                    [new DateOnly(2026, 4, 1)] = 19100,
                    [new DateOnly(2026, 5, 1)] = 19700,
                    [new DateOnly(2026, 6, 1)] = 20150,
                    [new DateOnly(2026, 7, 1)] = 20550,
                    [new DateOnly(2026, 8, 1)] = 21100,
                    [new DateOnly(2026, 9, 1)] = 21650,
                }),
                
                YearColumn = ExcelColumn.FromString("A"),
                MonthColumn = ExcelColumn.FromString("B"),
                DateColumn = ExcelColumn.FromString("C"),
                CategoryColumn = ExcelColumn.FromString("D"),
                SubCategoryColumn = ExcelColumn.FromString("E"),
                ShopColumn = ExcelColumn.FromString("F"),
                DescriptionColumn = ExcelColumn.FromString("G"),
                AmountRurColumn = ExcelColumn.FromString("H"),
                AmountAmdColumn = ExcelColumn.FromString("I"),
                OtherAmountColumn = ExcelColumn.FromString("J"),
                OtherCurrencyColumn = ExcelColumn.FromString("K"),
            };
            instance.FlatInfo = new ListInfo()
            {
                ListName = "Квартира",
                Category = "Квартира",
                YearColumn = ExcelColumn.FromString("A"),
                MonthColumn = ExcelColumn.FromString("B"),
                DateColumn = ExcelColumn.FromString("C"),
                SubCategoryColumn = ExcelColumn.FromString("D"),
                ShopColumn = ExcelColumn.FromString("E"),
                DescriptionColumn = ExcelColumn.FromString("F"),
                AmountRurColumn = ExcelColumn.FromString("G"),
                AmountAmdColumn = ExcelColumn.FromString("H"),
            };
            instance.BigDealInfo = new ListInfo()
            {
                ListName = "Крупные",
                Category = "Крупные",
                DateColumn = ExcelColumn.FromString("C"),
                CategoryColumn = ExcelColumn.FromString("D"),
                DescriptionColumn = ExcelColumn.FromString("E"),
                AmountRurColumn = ExcelColumn.FromString("F"),
                AmountAmdColumn = ExcelColumn.FromString("G"),
                OtherAmountColumn = ExcelColumn.FromString("H"),
                OtherCurrencyColumn = ExcelColumn.FromString("I"),
            };
            instance.CurrencyConversion = new CurrencyExchangeListInfo()
            {
                ListName = "Обмен валюты",
                Category = "Обмен валюты",
                DateColumn = ExcelColumn.FromString("C"),
                ShopColumn = ExcelColumn.FromString("D"),
                DescriptionColumn = ExcelColumn.FromString("E"),
                SourceAmountColumn = ExcelColumn.FromString("F"),
                SourceCurrencyColumn = ExcelColumn.FromString("G"),
                TargetAmountColumn = ExcelColumn.FromString("I"),
                TargetCurrencyColumn = ExcelColumn.FromString("J"),
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
            };
            instance.FutureExpenses = new FutureExpenseListInfo()
            {
                ListName = "Обязательные траты",
                
                NameColumn = ExcelColumn.FromString("A"),
                CategoryColumn = ExcelColumn.FromString("B"),
                SubCategoryColumn = ExcelColumn.FromString("C"),
                ShopColumn = ExcelColumn.FromString("D"),
                FrequencyColumn = ExcelColumn.FromString("E"),
                WayColumn = ExcelColumn.FromString("F"),
                SumColumn = ExcelColumn.FromString("G"),
                CurrencyColumn = ExcelColumn.FromString("H"),
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
                    ["Операция"] = "Крупные",
                };

                return instance;
            }
        );

        services.AddSingleton<IExpenseRepository, ExpenseSheetRepository>();
        services.AddSingleton<IIncomeRepository, IncomeSheetRepository>();
        services.AddSingleton<ISheetRepository<CurrencyExchange>, CurrencyExchangeFromSheetRepository>();
        services.AddSingleton<IFutureExpenseRepository, FutureExpenseRepository>();
        services.AddScoped<GoogleSheetAzureFunction>();
    })
    .Build();

host.Run();