using Application.Core;
using Domain;

namespace Application.Bot.Flows;

public record StatisticsQuery(
    DateRange Period,
    MonthRange MonthRange,
    Category? Category,
    SubCategory? SubCategory,
    Currency Currency
);