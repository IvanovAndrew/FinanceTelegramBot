using Domain;

namespace Application.Core;

public record StatisticsQuery(
    DateRange Period,
    MonthRange MonthRange,
    Category? Category,
    SubCategory? SubCategory,
    Currency Currency
);