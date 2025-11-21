using MediatR;

namespace Application.Statistic.StatisticBySubcategory;

public record StatisticBySubcategoryDateFromSavedEvent : INotification
{
    public long SessionId { get; init; }
}