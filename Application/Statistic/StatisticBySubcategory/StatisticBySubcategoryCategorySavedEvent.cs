using MediatR;

namespace Application.Statistic.StatisticBySubcategory;

public record StatisticBySubcategoryCategorySavedEvent : INotification
{
    public long SessionId { get; init; }
}