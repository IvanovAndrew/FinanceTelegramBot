using MediatR;

namespace Application.Statistic.StatisticBySubcategoryByMonth;

public class StatisticBySubcategoryMonthCategorySavedEvent : INotification
{
    public long SessionId { get; init; }
}