using Domain;

namespace Infrastructure;

public interface IStateFactory
{
    IExpenseInfoState CreateGreetingState();

    IExpenseInfoState WayOfEnteringExpenseState();
    IExpenseInfoState CreateRequestPasteJsonState();
    IExpenseInfoState CreateChooseStatisticState();
    IExpenseInfoState CreateCollectDayExpenseState();
    IExpenseInfoState CreateConfirmState(IExpense expense);
    IExpenseInfoState CreateConfirmState(IIncome income);
    IExpenseInfoState CreateSaveState(IExpense expense);
    IExpenseInfoState CreateSaveState(IIncome expense);
    IExpenseInfoState CreateSaveExpensesFromJsonState(List<IExpense> expenses);
    IExpenseInfoState CreateHandleJsonFileState(ITelegramFileInfo fileInfo);
    IExpenseInfoState CreateErrorWithRetryState(string warning, IExpenseInfoState state);
    IExpenseInfoState CreateCancelState();

    IExpenseInfoState GetExpensesState<T>(IExpenseInfoState expenseInfoState, FinanceFilter financeFilter,
        ExpensesAggregator<T> expensesAggregator, Func<T, string> firstColumnName, TableOptions tableOptions);

    IExpenseInfoState CreateCollectMonthStatisticState();

    IExpenseInfoState
        CreateCollectCategoryExpensesByMonthsState();

    IExpenseInfoState CreateCollectSubcategoryExpensesByMonthsState();

    IExpenseInfoState CreateCollectSubcategoriesForAPeriodState();
    IExpenseInfoState CreateEnterRawQrState();
    IExpenseInfoState CreateCheckInfoState();
    IExpenseInfoState CreateCheckByRequisitesState();
    IExpenseInfoState CreateRequestFnsDataState(string messageText);
    IExpenseInfoState CreateEnterIncomeState();
    IExpenseInfoState CreateEnterOutcomeManuallyState();
}