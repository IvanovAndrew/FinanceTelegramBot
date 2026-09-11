using Domain;
using MediatR;

namespace Application.Api;

public record GetSpendingDailyHistoryApiCommand : IRequest<SpendingDailyHistoryResult>
{
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }
    public Currency Currency { get; init; }
}

public record SpendingDailyHistoryResult(Currency Currency, IReadOnlyList<DaySpending> Days);

public record DaySpending(DateOnly Day, Money Total, IReadOnlyList<ShopSpending> ShopChecks);

public record ShopSpending(Shop Shop, IReadOnlyList<CategorySpending> Categories);