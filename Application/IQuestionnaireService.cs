using Application.AddMoneyTransfer;
using Application.AddMoneyTransferByRequisites;
using Application.Commands.StatisticByDay;
using Application.Statistic.StatisticBalance;
using Application.Statistic.StatisticByCategory;
using Application.Statistic.StatisticByDay;
using Application.Statistic.StatisticByMonth;
using Application.Statistic.StatisticBySubcategory;
using Application.Statistic.StatisticBySubcategoryByMonth;
using MediatR;

namespace Application;

public interface IQuestionnaireService
{
    IRequest GetHandler(long sessionId, string text);
    void Back();
    void Next();
}

public class ManualMoneyTransferQuestionnaireService : IQuestionnaireService
{
    private Dictionary<int, Func<long, string, IRequest>> _handlers =
        new()
        {
            [0] = (sessionId, input) => new AddMoneyTransferDateCommand(){SessionId = sessionId, DateText = input},
            [1] = (sessionId, input) => new AddMoneyTransferCategoryCommand(){SessionId = sessionId, Category = input},
            [2] = (sessionId, input) => new AddMoneyTransferSubcategoryCommand(){SessionId = sessionId, Subcategory = input},
            [3] = (sessionId, input) => new AddMoneyTransferDescriptionCommand(){SessionId = sessionId, Description = input},
            [4] = (sessionId, input) => new AddMoneyTransferPriceCommand(){SessionId = sessionId, Price = input},
        };

    private int _current = 0;
    
    public IRequest GetHandler(long sessionId, string text)
    {
        return _handlers[_current](sessionId, text);
    }

    public void Back()
    {
        _current--;
    }

    public void Next()
    {
        _current++;
    }
}

public class ManualRequisitesQuestionnaireService : IQuestionnaireService
{
    private int _current = 0;
    
    private Dictionary<int, Func<long, string, IRequest>> _handlers =
        new()
        {
            [0] = (sessionId, input) => new AddCheckDateCommand(){SessionId = sessionId, Date = input},
            [1] = (sessionId, input) => new AddCheckTotalPriceCommand(){SessionId = sessionId, Price = input},
            [2] = (sessionId, input) => new AddCheckFiscalNumberCommand(){SessionId = sessionId, FiscalNumber = input},
            [3] = (sessionId, input) => new AddCheckFiscalDocumentNumberCommand(){SessionId = sessionId, DocumentNumber = input},
            [4] = (sessionId, input) => new AddCheckFiscalDocumentSignCommand(){SessionId = sessionId, DocumentSign = input},
        };
    
    public IRequest GetHandler(long sessionId, string text)
    {
        return _handlers[_current](sessionId, text);
    }

    public void Back()
    {
        _current--;
    }

    public void Next()
    {
        _current++;
    }
}

public class DayStatisticQuestionnaire : IQuestionnaireService
{
    private int _current = 0;
    
    private Dictionary<int, Func<long, string, IRequest>> _handlers =
        new()
        {
            [0] = (sessionId, input) => new StatisticByDaySaveDateCommand() {SessionId = sessionId, Date = input},
            [1] = (sessionId, input) => new StatisticByDaySaveCurrencyCommand() {SessionId = sessionId, Currency = input}
        };
    
    public IRequest GetHandler(long sessionId, string text)
    {
        return _handlers[_current](sessionId, text);
    }

    public void Back()
    {
        _current--;
    }

    public void Next()
    {
        _current++;
    }
}

public class StatisticBalanceQuestionnaire : IQuestionnaireService
{
    private int _current = 0;
    
    private Dictionary<int, Func<long, string, IRequest>> _handlers =
        new()
        {
            [0] = (sessionId, input) => new StatisticBalanceSaveDateCommand() {SessionId = sessionId, DateFrom = input},
            [1] = (sessionId, input) => new StatisticBalanceSaveCurrencyCommand() {SessionId = sessionId, Currency = input}
        };
    
    public IRequest GetHandler(long sessionId, string text)
    {
        return _handlers[_current](sessionId, text);
    }

    public void Back()
    {
        _current--;
    }

    public void Next()
    {
        _current++;
    }
}

public class CategoryStatisticQuestionnaire : IQuestionnaireService
{
    private int _current = 0;
    
    private Dictionary<int, Func<long, string, IRequest>> _handlers =
        new()
        {
            [0] = (sessionId, input) => new StatisticByCategorySaveCategoryCommand() {SessionId = sessionId, Category = input},
            [1] = (sessionId, input) => new StatisticByCategorySaveDateCommand() {SessionId = sessionId, DateText = input},
            [2] = (sessionId, input) => new StatisticByCategorySaveCurrencyCommand() {SessionId = sessionId, Currency = input}
        };
    
    public IRequest GetHandler(long sessionId, string text)
    {
        return _handlers[_current](sessionId, text);
    }

    public void Back()
    {
        _current--;
    }

    public void Next()
    {
        _current++;
    }
} 

public class MonthStatisticQuestionnaire : IQuestionnaireService
{
    private int _current = 0;
    
    private Dictionary<int, Func<long, string, IRequest>> _handlers =
        new()
        {
            [0] = (sessionId, input) => new StatisticByMonthSaveDateCommand() {SessionId = sessionId, Date = input},
            [1] = (sessionId, input) => new StatisticByMonthSaveCurrencyCommand() {SessionId = sessionId, Currency = input}
        };
    
    public IRequest GetHandler(long sessionId, string text)
    {
        return _handlers[_current](sessionId, text);
    }

    public void Back()
    {
        _current--;
    }

    public void Next()
    {
        _current++;
    }
}

public class SubcategoryStatisticQuestionnaire : IQuestionnaireService
{
    private int _current = 0;
    
    private Dictionary<int, Func<long, string, IRequest>> _handlers =
        new()
        {
            [0] = (sessionId, input) => new StatisticBySubcategorySaveCategoryCommand() {SessionId = sessionId, Category = input},
            [1] = (sessionId, input) => new StatisticBySubcategorySaveDateCommand() {SessionId = sessionId, DateFromText = input},
            [2] = (sessionId, input) => new StatisticBySubcategorySaveCurrencyCommand() {SessionId = sessionId, Currency = input}
        };
    
    public IRequest GetHandler(long sessionId, string text)
    {
        return _handlers[_current](sessionId, text);
    }

    public void Back()
    {
        _current--;
    }

    public void Next()
    {
        _current++;
    }
} 

public class SubcategoryByMonthStatisticQuestionnaire : IQuestionnaireService
{
    private int _current = 0;
    
    private Dictionary<int, Func<long, string, IRequest>> _handlers =
        new()
        {
            [0] = (sessionId, input) => new StatisticBySubcategoryMonthSaveCategoryCommand() {SessionId = sessionId, Category = input},
            [1] = (sessionId, input) => new StatisticBySubcategoryMonthSaveSubcategoryCommand() {SessionId = sessionId, Subcategory = input},
            [2] = (sessionId, input) => new StatisticBySubcategoryMonthSaveDateCommand() {SessionId = sessionId, DateFromText = input},
            [3] = (sessionId, input) => new StatisticBySubcategoryMonthSaveCurrencyCommand() {SessionId = sessionId, Currency = input}
        };
    
    public IRequest GetHandler(long sessionId, string text)
    {
        return _handlers[_current](sessionId, text);
    }

    public void Back()
    {
        _current--;
    }

    public void Next()
    {
        _current++;
    }
}