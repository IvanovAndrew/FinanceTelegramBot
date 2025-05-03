using MediatR;

namespace Application.Statistic.StatisticBySubcategoryByMonth;

public class StatisticBySubcategoryMonthSubcategorySavedEvent : INotification
{
    public long SessionId { get; init; }
}