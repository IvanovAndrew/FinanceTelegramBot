using Domain;
using Infrastructure.Telegram;

namespace Infrastructure;

public interface IStateFactory
{
    IExpenseInfoState CreateGreetingState();

    IExpenseInfoState WayOfEnteringExpenseState();
    IExpenseInfoState CreateRequestPasteJsonState();
    IExpenseInfoState CreateChooseStatisticState();
    IExpenseInfoState CreateCollectDayExpenseState();
    IExpenseInfoState CreateConfirmState(IMoneyTransfer expense);
    IExpenseInfoState CreateSaveState(IMoneyTransfer expense);
    IExpenseInfoState CreateSaveExpensesFromJsonState(List<IMoneyTransfer> expenses);
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
    IExpenseInfoState CreateBalanceState();
    IExpenseInfoState CreateBalanceStatisticState(FinanceFilter filter);
}