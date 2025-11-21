using MediatR;

namespace Application.Statistic.StatisticBySubcategoryByMonth;

public record StatisticBySubcategoryMonthCategorySavedEvent : INotification
{
    public long SessionId { get; init; }
}