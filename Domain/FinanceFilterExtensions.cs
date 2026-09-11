namespace Domain;

public static class FinanceFilterExtensions
{
    /// <summary>
    /// Верно, если всё, что удовлетворяет requested, обязательно удовлетворяет и cached —
    /// то есть по данным, закэшированным под cached, можно ответить на requested без похода в репозиторий.
    /// </summary>
    public static bool IsSupersetOf(this FinanceFilter cached, FinanceFilter requested)
    {
        if (cached.DateFrom is { } cachedFrom &&
            (requested.DateFrom is not { } requestedFrom || cachedFrom > requestedFrom))
            return false;

        if (cached.DateTo is { } cachedTo &&
            (requested.DateTo is not { } requestedTo || cachedTo < requestedTo))
            return false;

        if (cached.Category is not null && !Equals(cached.Category, requested.Category)) return false;
        if (cached.Subcategory is not null && !Equals(cached.Subcategory, requested.Subcategory)) return false;
        if (cached.Currency is not null && !Equals(cached.Currency, requested.Currency)) return false;

        return true;
    }

    public static bool Matches(this IMoneyTransfer transfer, FinanceFilter filter)
    {
        if (filter.DateFrom is { } from && transfer.Date < from) return false;
        if (filter.DateTo is { } to && transfer.Date > to) return false;
        if (filter.Category is not null && !Equals(transfer.Category, filter.Category)) return false;
        if (filter.Subcategory is not null && !Equals(transfer.SubCategory, filter.Subcategory)) return false;
        if (filter.Currency is not null && transfer.Amount.Currency != filter.Currency) return false;
        return true;
    }
}