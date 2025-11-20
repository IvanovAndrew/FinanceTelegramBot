using Application.Contracts;
using Domain;

namespace Application;

public interface ICheckDownloader
{
    Currency Currency { get; }
    Task<List<Outcome>> DownloadExpenses(CheckRequisite checkRequisite, IExpenseCategorizer expenseCategorizer,
        Dictionary<string?, ExpenseCategorizerResult> dict, Category defaultCategory);
}

public class CheckDownloader(IFnsAPIService fnsApiService) : ICheckDownloader
{
    public Currency Currency => Currency.RUR;

    public async Task<List<Outcome>> DownloadExpenses(CheckRequisite checkRequisite,
        IExpenseCategorizer expenseCategorizer, Dictionary<string?, ExpenseCategorizerResult> availableOptions,
        Category defaultCategory)
    {
        var rawOutcomeItems = await fnsApiService.GetCheck(checkRequisite);
        
        List<Outcome> outcomes = new List<Outcome>();
        foreach (var rawOutcome in rawOutcomeItems)
        {
            var expenseCategoryResult = expenseCategorizer.GetCategory(rawOutcome.Description, availableOptions);

            outcomes.Add(
                new Outcome()
                {
                    Date = rawOutcome.Date,
                    Category = expenseCategoryResult?.Category?? defaultCategory,
                    SubCategory = expenseCategoryResult?.SubCategory,
                    Description = rawOutcome.Description,
                            
                    Amount = new Money(){Amount = rawOutcome.Amount, Currency = Currency},
                });
        }

        return outcomes;
    }
}