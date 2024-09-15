using Domain;
using Infrastructure.Telegram;

namespace Infrastructure;

public interface IStateFactory
{
    IExpenseInfoState CreateGreetingState();

    IExpenseInfoState WayOfEnteringExpenseState();
    IExpenseInfoState CreateRequestPasteJsonState();
    IExpenseInfoState CreateEnterTheDateState(IExpenseInfoState previousState, bool askCustomDate = false);
    IExpenseInfoState CreateChooseStatisticState();
    IExpenseInfoState CreateCollectDayExpenseState();
    IExpenseInfoState CreateEnterTheCategoryState(ExpenseBuilder expenseBuilder);

    IExpenseInfoState CreateEnterTheSubcategoryState(ExpenseBuilder expenseBuilder, SubCategory[] subCategories);
    IExpenseInfoState CreateEnterDescriptionState(ExpenseBuilder expenseBuilder);
    IExpenseInfoState CreateEnterThePriceState(ExpenseBuilder expenseBuilder);
    IExpenseInfoState CreateConfirmState(IExpense expense);
    IExpenseInfoState CreateSaveState(IExpense expense);
    IExpenseInfoState CreateSaveExpensesFromJsonState(List<IExpense> expenses);
    IExpenseInfoState CreateHandleJsonFileState(ITelegramFileInfo fileInfo);
    IExpenseInfoState CreateErrorWithRetryState(string warning, IExpenseInfoState state);
    IExpenseInfoState CreateCancelState();

    IExpenseInfoState GetExpensesState<T>(IExpenseInfoState expenseInfoState, ExpenseFilter expenseFilter,
        ExpensesAggregator<T> expensesAggregator, Func<T, string> firstColumnName, TableOptions tableOptions);

    IExpenseInfoState CreateCollectMonthStatisticState();

    IExpenseInfoState
        CreateCollectCategoryExpensesByMonthsState();

    IExpenseInfoState CreateCollectSubcategoryExpensesByMonthsState();

    IExpenseInfoState CreateCollectSubcategoriesForAPeriodState();
    IExpenseInfoState CreateEnterRawQrState();
    IExpenseInfoState CreateCheckInfoState();
}