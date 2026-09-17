using Application.Bot.Statistic;
using Application.Core;
using Application.Core.Services;
using Application.Core.Statistic;
using Domain;
using MediatR;

namespace Application.Bot.Flows;

public sealed class StatisticsFlow : UserFlow
{
    private readonly IDateTimeService _dateTimeService;
    private readonly StatisticsFlowResolver _resolver;
    
    public StatisticsQueryDraft Draft { get; } = new();

    public StatisticsQuery ToEntity() => Draft.Build();

    public StatisticsFlow(IDateTimeService dateTimeService)
    {
        _dateTimeService = dateTimeService;

        _resolver = new StatisticsFlowResolver();
    }
    
    public override void ComputeStep()
    {
        var localStep = _resolver.Resolve(Draft);
        CurrentStep = Map(localStep);
    }
    
    private static FlowStep Map(StatisticFlowStep step) =>
        step switch
        {
            StatisticFlowStep.AskStatisticsMode => FlowStep.AskStatisticMode,
            StatisticFlowStep.AskDay => FlowStep.AskDay,
            StatisticFlowStep.AskCustomDay => FlowStep.AskCustomDay,
            StatisticFlowStep.AskMonth => FlowStep.AskMonth,
            StatisticFlowStep.AskCustomMonth => FlowStep.AskCustomMonth,
            StatisticFlowStep.AskCategory => FlowStep.AskOutcomeCategory,
            StatisticFlowStep.AskSubcategory => FlowStep.AskSubCategory,
            StatisticFlowStep.AskCurrency => FlowStep.AskCurrency,
            StatisticFlowStep.Done => FlowStep.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(step))
        };

    public override Task HandleInput(FlowStep step, string text, CancellationToken ct)
    {
        switch (step)
        {
            case FlowStep.AskDay:
            case FlowStep.AskCustomDay:
            {
                if (!_dateTimeService.TryParseDate(text, out var date))
                {
                    Draft.CustomDayMode = true;
                }
                else
                {
                    Draft.SetFromDate(date);
                }
                break;
            }
            
            case FlowStep.AskMonth:
            case FlowStep.AskCustomMonth:
            {
                if (!_dateTimeService.TryParseDate(text, out var date))
                {
                    Draft.CustomDayMode = true;
                }
                else
                {
                    Draft.Month = YearMonth.From(date);
                }
                break;
            }
            
            case FlowStep.AskOutcomeCategory:
            case FlowStep.AskIncomeCategory:
                Draft.SetCategory(ResolveCategory(text));
                break;

            case FlowStep.AskSubCategory:
                Draft.SetSubCategory(ResolveSubCategory(Draft.Category, text));
                break;
            
            case FlowStep.AskCurrency:

                if (string.Equals(text, "Show All", StringComparison.InvariantCultureIgnoreCase))
                {
                    Draft.ShowAllCurrencies = true;
                }
                else if (Currency.TryParse(text, out var currency))
                {
                    Draft.SetCurrency(currency);
                }
                else if (string.Equals(text, "all", StringComparison.InvariantCultureIgnoreCase))
                {
                    Draft.SetCurrency(true);
                }
                
                break;
            
            case FlowStep.Confirm:
            {
                break;
            }
            
            default:
                throw new InvalidOperationException(step.ToString());
        }
        
        return Task.CompletedTask;
    }

    public override Task Complete(long sessionId, IMediator mediator, CancellationToken ct)
    {
        var statisticsQuery = ToEntity();

        IBaseRequest command = Draft.Mode switch
        {
            StatisticsQueryMode.DailyExpenses => new StatisticDayRequestCommand { SessionId = sessionId, Query = statisticsQuery },
            StatisticsQueryMode.DayByDayExpenses => new StatisticDayByDayRequestCommand { SessionId = sessionId, Query = statisticsQuery },
            StatisticsQueryMode.BalanceFromMonth => new GetBalanceStatisticCommand { SessionId = sessionId, Query = statisticsQuery },
            StatisticsQueryMode.MonthlyExpenses => new GetStatisticMonthRequestCommand { SessionId = sessionId, Query = statisticsQuery },
            StatisticsQueryMode.CategoryByMonths => new GetStatisticCategoryRequestCommand { SessionId = sessionId, Query = statisticsQuery },
            StatisticsQueryMode.SubcategoryByMonth => new StatisticSubcategoryMonthRequestCommand { SessionId = sessionId, Query = statisticsQuery },
            StatisticsQueryMode.SubcategoryTotal => new StatisticSubcategoryRequestCommand { SessionId = sessionId, Query = statisticsQuery },
            _ => throw new InvalidOperationException($"Unknown statistics mode: {Draft.Mode}")
        };

        return mediator.Send(command, ct);
    }

    internal override ExtraData GetExtraData()
    {
        return new ExtraData() { Category = Draft.Category, ShowAllCurrencies = Draft.ShowAllCurrencies, ShowAllCategories = true };
    }

    private Category? ResolveCategory(string input)
    {
        var category = Categories.Outcome.GetCategory(input);

        return category;
    }
    
    private SubCategory? ResolveSubCategory(Category category, string input)
    {
        return category.Sub(input);
    }
}

public enum StatisticsQueryMode
{
    None,
    DailyExpenses,
    DayByDayExpenses,
    MonthlyExpenses,
    BalanceFromMonth,
    CategoryByMonths,
    SubcategoryByMonth,
    SubcategoryTotal
}

public class StatisticsQueryDraft
{
    internal StatisticsQueryMode Mode { get; set; }
    private DateOnly? _from;
    private DateOnly? _to;

    public DateOnly? Day { get; set; }
    public YearMonth? Month { get; set; }
    
    public DateRange? Period =>
        _from.HasValue && _to.HasValue
            ? new DateRange(_from.Value, _to.Value)
            : null;

    public Category? Category { get; internal set; }
    public SubCategory? SubCategory { get; private set; }
    public Currency? Currency { get; private set; }
    public bool IsCurrencySpecified { get; private set; }
    public bool ShowAllCurrencies { get; internal set; }

    // --- Date ---

    public bool HasFromDate => _from.HasValue;
    public bool HasToDate => _to.HasValue;
    public bool HasPeriod => HasFromDate && HasToDate;
    public bool CustomDayMode { get; set; }

    public void SetFromDate(DateOnly from)
    {
        _from = from;

        if (_to.HasValue && _to < _from)
            _to = _from;
    }

    public void SetToDate(DateOnly to)
    {
        _to = to;

        if (_from.HasValue && _to < _from)
            _from = _to;
    }

    public void SetPeriod(DateOnly from, DateOnly to)
    {
        if (to < from)
            throw new ArgumentException("Invalid period");

        _from = from;
        _to = to;
    }

    // --- Filters ---

    public void SetCategory(Category category)
    {
        Category = category;
        SubCategory = null;
    }

    public void SetSubCategory(SubCategory subCategory)
    {
        if (Category == null)
            throw new InvalidOperationException();

        SubCategory = subCategory;
    }

    public void SetCurrency(Currency currency)
    {
        Currency = currency;
        IsCurrencySpecified = true;
    }
    
    public void SetCurrency(bool isSpecified = true)
    {
        IsCurrencySpecified = isSpecified;
    }

    // --- Finalization ---

    public StatisticsQuery Build()
    {
        DateRange dateRange = default;
        MonthRange monthRange = default;
        if (Day != null)
        {
            dateRange = new DateRange(Day.Value, Day.Value);
        }
        else
        {
            monthRange = new MonthRange() { From = Month.Value };
            if (Mode == StatisticsQueryMode.MonthlyExpenses)
            {
                monthRange = monthRange with {To = Month.Value};
            }
        }
        
        
        
        return new StatisticsQuery(
            dateRange,
            monthRange,
            Category,
            SubCategory,
            Currency
        );
    }
}

public enum StatisticFlowStep
{
    AskStatisticsMode,
    AskDay,
    AskCustomDay,
    AskMonth,
    AskCustomMonth,
    AskCategory,
    AskSubcategory,
    AskCurrency,
    Done
}

public class StatisticsFlowResolver
{
    private enum RequiredField { Category, Subcategory, Day, Month, Currency }

    private static readonly Dictionary<StatisticsQueryMode, RequiredField[]> Requirements = new()
    {
        [StatisticsQueryMode.DailyExpenses]        = [RequiredField.Day, RequiredField.Currency],
        [StatisticsQueryMode.DayByDayExpenses]     = [RequiredField.Month, RequiredField.Currency],
        [StatisticsQueryMode.MonthlyExpenses]      = [RequiredField.Month, RequiredField.Currency],
        [StatisticsQueryMode.BalanceFromMonth]     = [RequiredField.Month, RequiredField.Currency],
        [StatisticsQueryMode.CategoryByMonths]     = [RequiredField.Category, RequiredField.Month, RequiredField.Currency],
        [StatisticsQueryMode.SubcategoryByMonth]   = [RequiredField.Category, RequiredField.Subcategory, RequiredField.Month, RequiredField.Currency],
        [StatisticsQueryMode.SubcategoryTotal]     = [RequiredField.Category, RequiredField.Month, RequiredField.Currency],
    };

    public StatisticFlowStep Resolve(StatisticsQueryDraft d)
    {
        if (d.Mode == StatisticsQueryMode.None)
            return StatisticFlowStep.AskStatisticsMode;

        foreach (var field in Requirements[d.Mode])
        {
            switch (field)
            {
                case RequiredField.Category when d.Category == null:
                    return StatisticFlowStep.AskCategory;
                case RequiredField.Subcategory when d.SubCategory == null:
                    return StatisticFlowStep.AskSubcategory;
                case RequiredField.Day when d.Day == null:
                    return d.CustomDayMode ? StatisticFlowStep.AskCustomDay : StatisticFlowStep.AskDay;
                case RequiredField.Month when d.Month == null:
                    return d.CustomDayMode ? StatisticFlowStep.AskCustomMonth : StatisticFlowStep.AskMonth;
                case RequiredField.Currency when !d.IsCurrencySpecified:
                    return StatisticFlowStep.AskCurrency;
                default:
                    continue;
            }
        }

        return StatisticFlowStep.Done;
    }
}