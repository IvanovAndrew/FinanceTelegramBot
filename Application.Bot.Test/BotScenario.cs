using Application.Bot;
using Application.Test.Extensions;
using Application.Test.Stubs;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Test;

public class BotScenario
{
    private readonly BotEngineWrapper _bot;
    private readonly BotTestEnvironment _environment;

    public DateTimeServiceStub Time => _environment.Time;
    public FinanceRepositoryStub Repo => _environment.Repo;
    public MessageServiceMock MessageService => _environment.MessageService;
    internal IOutcomingMessage LastMessage => MessageService.SentMessages.Last();
    public SalaryDayServiceStub SalaryDayService => _environment.SalaryDayService;
    public FnsReceiptProviderStub FnsReceiptProvider => _environment.FnsReceiptProvider;
    public YerevanCityReceiptProviderStub YerevanCityReceiptProvider => _environment.YerevanCityReceiptProvider;
    public ExpenseJsonParserStub ExpenseJsonParser => _environment.ExpenseJsonParser;

    private BotScenario(BotEngineWrapper bot, BotTestEnvironment environment)
    {
        _bot = bot;
        _environment = environment;
    }

    public static BotScenario Preset()
    {
        var (provider, env) = TestServiceFactory.Create();
        return new BotScenario(provider.GetRequiredService<BotEngineWrapper>(), env);
    }

    public static async Task<BotScenario> Start()
    {
        var scenario = Preset();
        await scenario._bot.Proceed("/start");
        return scenario;
    }

    public async Task ChooseOutcome()
    {
        await _bot.Proceed("outcome");
    }

    public async Task EnterManually()
    {
        await _bot.Proceed("By myself");
    }

    public async Task WithDateToday()
    {
        await _bot.Proceed("today");
    }
    
    public async Task WithDateYesterday()
    {
        await _bot.Proceed("yesterday");
    }
    
    public async Task WithCustomDate(string input)
    {
        await _bot.Proceed("Another day");
        await _bot.Proceed(input);
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
        await _bot.Proceed(month);
    }
    
    public async Task WithCustomMonth(string month)
    {
        await _bot.Proceed("Another month");
        await _bot.Proceed(month);
    }

    public async Task WithCategory(string category)
    {
        await _bot.Proceed(category);
    }
    
    public async Task WithSubCategory(string subcategory)
    {
        await _bot.Proceed(subcategory);
    }

    public async Task WithDescription(string description)
    {
        await _bot.Proceed(description);
    }

    public async Task WithPrice(string price)
    {
        await _bot.Proceed(price);
    }

    public async Task ConfirmSaving()
    {
        await _bot.Proceed("Save");
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
        await _bot.Proceed("From check");
        await _bot.Proceed("json");
    }

    public async Task LoadFile(FileInfoStub telegramFile)
    {
        await _bot.ProceedFile(telegramFile);
    }

    public async Task ChooseCheckByFNSRequisites()
    {
        await _bot.Proceed("From check");
        await _bot.Proceed("By Requisites");
        await _bot.Proceed("Russia");
    }
    
    public async Task ChooseCheckByYerevanCityRequisites()
    {
        await _bot.Proceed("From check");
        await _bot.Proceed("By Requisites");
        await _bot.Proceed("Yerevan city");
    }

    public async Task WithFiscalNumber(string fiscalNumber)
    {
        await _bot.Proceed(fiscalNumber);
    }

    public async Task WithFiscalDocument(string fiscalDocument)
    {
        await _bot.Proceed(fiscalDocument);
    }

    public async Task WithFiscalDocumentSign(string fiscalDocumentSign)
    {
        await _bot.Proceed(fiscalDocumentSign);
    }

    public async Task WithCheckId(string s)
    {
        await _bot.Proceed(s);
    }
}