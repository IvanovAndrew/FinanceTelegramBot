namespace Application;

public enum AddMoneyTransferStep
{
    AskDate,
    AskCustomDate,
    AskOutcomeCategory,
    AskIncomeCategory,
    AskSubCategory,
    AskDescription,
    AskAmount,
    Confirm,
    Save,
}

internal class AddMoneyTransferFlowResolver
{
    public AddMoneyTransferStep Resolve(MoneyTransferDraft d)
    {
        if (d.Date == null) return d.CustomDayMode? AddMoneyTransferStep.AskCustomDate : AddMoneyTransferStep.AskDate;
        
        if (d.Category == null) return d.IsIncome? AddMoneyTransferStep.AskIncomeCategory : AddMoneyTransferStep.AskOutcomeCategory;
        if (d.Category.Subcategories.Any() && d.SubCategory == null) return AddMoneyTransferStep.AskSubCategory;
        
        if (string.IsNullOrEmpty(d.Description)) return AddMoneyTransferStep.AskDescription;
        if (d.Sum == null) return AddMoneyTransferStep.AskAmount;
        if (!d.IsComplete) return AddMoneyTransferStep.Confirm;

        return AddMoneyTransferStep.Save;
    }
}