using Domain;
using MediatR;

namespace Application.Api;

public record GetSpendingHistoryApiCommand : IRequest<SpendingHistoryResult>
{
    public YearMonth StartMonth { get; init; }
    public Currency Currency { get; init; }
}

public record SpendingHistoryResult(Currency Currency, IReadOnlyList<MonthSpending> Months);

public record MonthSpending(
    YearMonth Month,
    Money Total,
    Money OutcomeTotal,
    Money RealOutcomeTotal,
    Money IncomeTotal,
    IReadOnlyList<CategorySpending> OutcomeCategories,
    IReadOnlyList<CategorySpending> IncomeCategories);

public record CategorySpending(Category Category, Money Total, IReadOnlyList<SubCategorySpending> SubCategories);

public record SubCategorySpending(SubCategory? SubCategory, Money Total);