using Domain;

namespace Infrastructure;

public interface IStateFactory
{
    IExpenseInfoState CreateGreetingState();

    IExpenseInfoState WayOfEnteringExpenseState();
    IExpenseInfoState CreateRequestPasteJsonState();
    IExpenseInfoState CreateEnterTheDateState(IExpenseInfoState previousState, bool askCustomDate = false);
    IExpenseInfoState CreateChooseStatisticState();
    IExpenseInfoState CreateCategoryForStatisticState();
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
    IExpenseInfoState CreateEnterTypeOfCategoryStatistic(Category category, IExpenseInfoState previousState);

    IExpenseInfoState CreateEnterTheCategoryForManyExpenses(List<IExpense> expenses);
    IExpenseInfoState CreateCollectMonthStatisticState();

    IExpenseInfoState
        CreateCollectCategoryExpensesByMonthsState(Category category);

    IExpenseInfoState CreateCollectSubcategoryExpensesByMonthsState(Category category, SubCategory subCategory);

    IExpenseInfoState CreateCollectCategoryExpensesBySubcategoriesForAPeriodState(Category category);
    IExpenseInfoState EnterSubcategoryStatisticState(IExpenseInfoState previousState, Category category);
    IExpenseInfoState CreateEnterRawQrState();
    IExpenseInfoState CreateCheckInfoState();
}