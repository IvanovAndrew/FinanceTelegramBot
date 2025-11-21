using MediatR;

namespace Application.Statistic.StatisticBySubcategoryByMonth;

public record StatisticBySubcategoryMonthSubcategorySavedEvent : INotification
{
    public long SessionId { get; init; }
}