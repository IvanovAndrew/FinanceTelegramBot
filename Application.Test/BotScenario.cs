using Application.Test.Extensions;
using Application.Test.Stubs;
using Microsoft.Extensions.DependencyInjection;
using UnitTest;

namespace Application.Test;

public class BotScenario
{
    private readonly BotEngineWrapper _bot;
    
    public DateTimeServiceStub Time { get; }
    public FinanceRepositoryStub Repo { get; }
    public MessageServiceMock MessageService { get; }
    internal IMessage LastMessage { get; private set; }
    public SalaryDayServiceStub SalaryDayService { get; private set; }
    public FnsApiServiceStub FnsService { get; private set; }

    private BotScenario(BotEngineWrapper bot, FinanceRepositoryStub repo, DateTimeServiceStub time, MessageServiceMock messageService, SalaryDayServiceStub salaryDayService, FnsApiServiceStub fnsApiServiceStub, IMessage message)
    {
        _bot = bot;
        Time = time;
        Repo = repo;
        MessageService = messageService;
        SalaryDayService = salaryDayService;
        FnsService = fnsApiServiceStub;
    }

    public static BotScenario Preset()
    {
        var provider = TestServiceFactory.Create(out var financeRepository, out var dateTimeServiceStub, out var messageService, out var fnsApiService, out var salaryDayService);
        var bot = provider.GetRequiredService<BotEngineWrapper>();
        
        return new BotScenario(bot, financeRepository, dateTimeServiceStub, messageService, salaryDayService, fnsApiService, default);
    }

    public static async Task<BotScenario> Start()
    {
        var provider = TestServiceFactory.Create(out var financeRepository, out var dateTimeServiceStub, out var messageService, out var fnsApiService, out var salaryDayService);
        var bot = provider.GetRequiredService<BotEngineWrapper>();

        var message = await bot.Proceed("/start");

        return new BotScenario(bot, financeRepository, dateTimeServiceStub, messageService, salaryDayService, fnsApiService, message);
    }

    public async Task ChooseOutcome()
    {
        LastMessage = await _bot.Proceed("outcome");
    }

    public async Task EnterManually()
    {
        LastMessage = await _bot.Proceed("By myself");
    }

    public async Task WithDateToday()
    {
        LastMessage = await _bot.Proceed("today");
    }
    
    public async Task WithDateYesterday()
    {
        LastMessage = await _bot.Proceed("yesterday");
    }
    
    public async Task WithCustomDate(string input)
    {
        LastMessage = await _bot.Proceed("Another day");
        LastMessage = await _bot.Proceed(input);
    }

    public Task WithDate(string date)
    {
        if (date == "today")
        {
            return WithDateToday();
        }

        if (date == "yesterday")
        {
            return WithDateYesterday();
        }

        return WithCustomDate(date);
    }

    public async Task WithMonth(string month)
    {
        LastMessage = await _bot.Proceed(month);
    }
    
    public async Task WithCustomMonth(string month)
    {
        LastMessage = await _bot.Proceed("Another month");
        LastMessage = await _bot.Proceed(month);
    }

    public async Task WithCategory(string category)
    {
        LastMessage = await _bot.Proceed(category);
    }
    
    public async Task WithSubCategory(string subcategory)
    {
        LastMessage = await _bot.Proceed(subcategory);
    }

    public async Task WithDescription(string description)
    {
        LastMessage = await _bot.Proceed(description);
    }

    public async Task WithPrice(string price)
    {
        LastMessage = await _bot.Proceed(price);
    }

    public async Task ConfirmSaving()
    {
        LastMessage = await _bot.Proceed("Save");
    }

    public async Task GoToPriceInput(string date, string? category = null, string? subcategory = null,
        string? description = null)
    {
        await WithDate(date);
        if (category == null) return;
        
        await WithCategory(category);
        if (subcategory == null) return;
        
        await WithSubCategory(subcategory);
        if (description == null) return;
        
        await WithDescription(description);
    }

    public async Task SelectStatistics()
    {
        await _bot.Proceed("Statistics");
    }
    
    public async Task SelectDayStatistics()
    {
        await _bot.Proceed("Day expenses (by categories)");
    }

    public Task WithAmdCurrency()
    {
        return WithCurrency("AMD");
    }

    public async Task WithCurrency(string currency)
    {
        await _bot.Proceed(currency);
    }

    public async Task Back()
    {
        await _bot.Proceed("/back");
    }

    public async Task Cancel()
    {
        await _bot.Proceed("/cancel");
    }

    public async Task SelectBalance()
    {
        await _bot.Proceed("Balance");
    }

    public async Task SelectCategoryByMonths()
    {
        await _bot.Proceed("Category expenses (by months)");
    }

    public async Task SelectMonthStatistics()
    {
        await _bot.Proceed("Month expenses (by categories)");
    }

    public async Task SelectSubcategoryByMonths()
    {
        await _bot.Proceed("Subcategory expenses (by months)");
    }

    public async Task SelectSubcategoryOverall()
    {
        await _bot.Proceed("Subcategory expenses (overall)");
    }

    public async Task ChooseIncome()
    {
        await _bot.Proceed("Income");
    }

    public async Task ChooseJsonCheck()
    {
        LastMessage = await _bot.Proceed("From check");
        LastMessage = await _bot.Proceed("json");
    }

    public async Task LoadFile(FileInfoStub telegramFile)
    {
        LastMessage = await _bot.ProceedFile(telegramFile);
    }

    public async Task ChooseCheckByRequisites()
    {
        LastMessage = await _bot.Proceed("From check");
        LastMessage = await _bot.Proceed("By Requisites");
    }

    public async Task WithFiscalNumber(string fiscalNumber)
    {
        LastMessage = await _bot.Proceed(fiscalNumber);
    }

    public async Task WithFiscalDocument(string fiscalDocument)
    {
        LastMessage = await _bot.Proceed(fiscalDocument);
    }

    public async Task WithFiscalDocumentSign(string fiscalDocumentSign)
    {
        LastMessage = await _bot.Proceed(fiscalDocumentSign);
    }
}