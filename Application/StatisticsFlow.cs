using Domain;

namespace Application;

public sealed class StatisticsFlow : UserFlow
{
    private readonly IDateTimeService _dateTimeService;
    private readonly ICategoryProvider _categoryProvider;
    private readonly StatisticsFlowResolver _resolver;
    
    public StatisticsQueryDraft Draft { get; } = new();

    public StatisticsQuery ToEntity() => Draft.Build();

    public StatisticsFlow(IDateTimeService dateTimeService, ICategoryProvider categoryProvider)
    {
        _dateTimeService = dateTimeService;
        _categoryProvider = categoryProvider;

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
            StatisticFlowStep.AskSubcategory => FlowStep.AskSubcategory,
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
                    Draft.Day = date;
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

            case FlowStep.AskSubcategory:
                Draft.SetSubCategory(ResolveSubCategory(Draft.Category, text));
                break;
            
            case FlowStep.AskCurrency:

                if (Currency.TryParse(text, out var currency))
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

    internal override ExtraData GetExtraData()
    {
        return new ExtraData() { Category = Draft.Category };
    }

    private Category? ResolveCategory(string input)
    {
        var category = _categoryProvider.GetCategoryByName(input, false);

        return category;
    }
    
    private SubCategory? ResolveSubCategory(Category category, string input)
    {
        return category.Subcategories.FirstOrDefault(s => s.Name == input);
    }
}

public record StatisticsQuery(
    DateRange Period,
    MonthRange MonthRange,
    Category? Category,
    SubCategory? SubCategory,
    Currency Currency
);

public record DateRange(DateOnly From, DateOnly To);


public enum StatisticsQueryMode
{
    None,
    DailyExpenses,
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
    public StatisticFlowStep Resolve(StatisticsQueryDraft d)
    {
        if (d.Mode == StatisticsQueryMode.None)
            return StatisticFlowStep.AskStatisticsMode;

        return d.Mode switch
        {
            StatisticsQueryMode.DailyExpenses => ResolveDaily(d),
            StatisticsQueryMode.MonthlyExpenses => ResolveMonthly(d),
            StatisticsQueryMode.BalanceFromMonth => ResolveBalance(d),
            StatisticsQueryMode.CategoryByMonths => ResolveCategoryByMonths(d),
            StatisticsQueryMode.SubcategoryByMonth => ResolveSubcategoryByMonth(d),
            StatisticsQueryMode.SubcategoryTotal => ResolveSubcategoryTotal(d),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private StatisticFlowStep ResolveDaily(StatisticsQueryDraft d)
    {
        if (d.Day == null)
            return d.CustomDayMode? StatisticFlowStep.AskCustomDay : StatisticFlowStep.AskDay;
        
        if (!d.IsCurrencySpecified)
            return StatisticFlowStep.AskCurrency;
        
        return StatisticFlowStep.Done;
    }
    
    private StatisticFlowStep ResolveMonthly(StatisticsQueryDraft d)
    {
        if (d.Month == null)
            return d.CustomDayMode? StatisticFlowStep.AskCustomMonth : StatisticFlowStep.AskMonth;
        
        if (!d.IsCurrencySpecified)
            return StatisticFlowStep.AskCurrency;
        
        return StatisticFlowStep.Done;
    }
    
    private StatisticFlowStep ResolveBalance(StatisticsQueryDraft d)
    {
        if (d.Month == null)
            return d.CustomDayMode? StatisticFlowStep.AskCustomMonth : StatisticFlowStep.AskMonth;
        
        if (!d.IsCurrencySpecified)
            return StatisticFlowStep.AskCurrency;
        
        return StatisticFlowStep.Done;
    }
    
    private StatisticFlowStep ResolveCategoryByMonths(StatisticsQueryDraft d)
    {
        if (d.Category == null)
            return StatisticFlowStep.AskCategory;
        
        if (d.Month == null)
            return d.CustomDayMode? StatisticFlowStep.AskCustomMonth : StatisticFlowStep.AskMonth;
        
        if (d.Currency == null)
            return StatisticFlowStep.AskCurrency;
        
        return StatisticFlowStep.Done;
    }
    
    private StatisticFlowStep ResolveSubcategoryByMonth(StatisticsQueryDraft d)
    {
        if (d.Category == null)
            return StatisticFlowStep.AskCategory;
        
        if (d.SubCategory == null)
            return StatisticFlowStep.AskSubcategory;
        
        if (d.Month == null)
            return d.CustomDayMode? StatisticFlowStep.AskCustomMonth : StatisticFlowStep.AskMonth;
        
        if (d.Currency == null)
            return StatisticFlowStep.AskCurrency;
        
        return StatisticFlowStep.Done;
    }
    
    private StatisticFlowStep ResolveSubcategoryTotal(StatisticsQueryDraft d)
    {
        if (d.Category == null)
            return StatisticFlowStep.AskCategory;
        
        if (d.Month == null)
            return d.CustomDayMode? StatisticFlowStep.AskCustomMonth : StatisticFlowStep.AskMonth;
        
        if (d.Currency == null)
            return StatisticFlowStep.AskCurrency;
        
        return StatisticFlowStep.Done;
    }
}