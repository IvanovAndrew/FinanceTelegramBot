using Domain;

namespace Infrastructure;

public interface IStateFactory
{
    IExpenseInfoState CreateGreetingState();

    IExpenseInfoState WayOfEnteringExpenseState(IExpenseInfoState previousState);
    IExpenseInfoState CreateRequestPasteJsonState(IExpenseInfoState previousState);
    IExpenseInfoState CreateEnterTheDateState(IExpenseInfoState previousState, bool askCustomDate = false);
    IExpenseInfoState CreateChooseStatisticState(IExpenseInfoState previousState);
    IExpenseInfoState CreateCategoryForStatisticState(IExpenseInfoState previousState);
    IExpenseInfoState CreateCollectDayExpenseState(IExpenseInfoState previousState);
    IExpenseInfoState CreateEnterTheCategoryState(ExpenseBuilder expenseBuilder, IExpenseInfoState previousState);

    IExpenseInfoState CreateEnterTheSubcategoryState(ExpenseBuilder expenseBuilder, IExpenseInfoState previousState,
        SubCategory[] subCategories);
    IExpenseInfoState CreateEnterDescriptionState(ExpenseBuilder expenseBuilder, IExpenseInfoState previousState);
    IExpenseInfoState CreateEnterThePriceState(ExpenseBuilder expenseBuilder, IExpenseInfoState previousState);
    IExpenseInfoState CreateConfirmState(IExpense expense, IExpenseInfoState previousState);
    IExpenseInfoState CreateSaveState(IExpenseInfoState previousState, IExpense expense);
    IExpenseInfoState CreateSaveExpensesFromJsonState(IExpenseInfoState previousState, List<IExpense> expenses);
    IExpenseInfoState CreateHandleJsonFileState(IExpenseInfoState previousState, ITelegramFileInfo fileInfo);
    IExpenseInfoState CreateErrorWithRetryState(string warning, IExpenseInfoState previousState);
    IExpenseInfoState CreateCancelState();

    IExpenseInfoState GetExpensesState<T>(IExpenseInfoState previousState, ISpecification<IExpense> specification,
        ExpensesAggregator<T> expensesAggregator, Func<T, string> firstColumnName, TableOptions tableOptions);
    IExpenseInfoState GetEnterTypeOfCategoryStatistic(IExpenseInfoState previousState, Category category);

    IExpenseInfoState CreateEnterTheCategoryForManyExpenses(List<IExpense> expenses,
        IExpenseInfoState previousState);
    IExpenseInfoState CreateCollectMonthStatisticState(IExpenseInfoState previousState);

    IExpenseInfoState
        CreateCollectCategoryExpensesByMonthsState(IExpenseInfoState previousState, Category category);

    IExpenseInfoState CreateCollectSubcategoryExpensesByMonthsState(IExpenseInfoState previousState,
        Category category, SubCategory subCategory);

    IExpenseInfoState CreateCollectCategoryExpensesBySubcategoriesForAPeriodState(IExpenseInfoState previousState,
        Category category);
    IExpenseInfoState EnterSubcategoryStatisticState(IExpenseInfoState previousState, Category category);
    IExpenseInfoState CreateEnterRawQrState(IExpenseInfoState previousState);
}