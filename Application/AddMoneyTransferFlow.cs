using Domain;
using Microsoft.Extensions.Logging;

namespace Application;

public class AddMoneyTransferFlow : UserFlow
{
    private readonly IDateTimeService _dateTimeService;
    private readonly ICategoryProvider _categoryProvider;
    private readonly ILogger _logger;
    private readonly AddMoneyTransferFlowResolver _resolver;
    
    public AddMoneyTransferFlow(bool isIncome, IDateTimeService dateTimeService, ICategoryProvider categoryProvider, ILogger logger)
    {
        _dateTimeService = dateTimeService;
        _categoryProvider = categoryProvider;
        
        Draft = new MoneyTransferDraft(isIncome);
        _resolver = new AddMoneyTransferFlowResolver();
        
        _logger = logger;
    }
    
    internal MoneyTransferDraft Draft { get; }
    public bool IsComplete => Draft.IsComplete;

    public IMoneyTransfer ToEntity() => Draft.ToEntity();
    
    public override void ComputeStep()
    {
        var localStep = _resolver.Resolve(Draft);
        CurrentStep = Map(localStep);
    }
    
    private static FlowStep Map(AddMoneyTransferStep step) =>
        step switch
        {
            AddMoneyTransferStep.AskDate => FlowStep.AskDay,
            AddMoneyTransferStep.AskCustomDate => FlowStep.AskCustomDay,
            AddMoneyTransferStep.AskOutcomeCategory => FlowStep.AskOutcomeCategory,
            AddMoneyTransferStep.AskIncomeCategory => FlowStep.AskIncomeCategory,
            AddMoneyTransferStep.AskSubCategory => FlowStep.AskSubcategory,
            AddMoneyTransferStep.AskDescription => FlowStep.AskDescription,
            AddMoneyTransferStep.AskAmount => FlowStep.AskAmount,
            AddMoneyTransferStep.Confirm => FlowStep.Confirm,
            AddMoneyTransferStep.Save => FlowStep.Completed,
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
                    _logger.LogWarning($"Couldn't parse {text} as a date");
                    Draft.CustomDayMode = true;
                }
                else
                {
                    Draft.SetDate(date);
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
            
            case FlowStep.AskDescription:
                Draft.SetDescription(text);
                break;

            case FlowStep.AskAmount:
                var result = Money.Parse(text);

                if (!result.IsSuccess)
                {
                    throw new FlowInputValidationException(step, result.Error);
                }

                Draft.SetAmount(result.Value);
                break;

            case FlowStep.Confirm:
            {
                if (text == "/save")
                    Draft.IsComplete = true;
                
                break;
            }
            
            default:
                throw new InvalidOperationException(step.ToString());
        }
        
        return Task.CompletedTask;
    }

    internal override ExtraData GetExtraData()
    {
        return new ExtraData(){Category = Draft.Category};
    }

    private Category? ResolveCategory(string input)
    {
        var category = _categoryProvider.GetCategoryByName(input, Draft.IsIncome);

        return category;
    }
    
    private SubCategory? ResolveSubCategory(Category category, string input)
    {
        return category.Subcategories.FirstOrDefault(s => s.Name == input);
    }
}