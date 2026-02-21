using Domain;
using MediatR;

namespace Application.Statistic.StatisticBySubcategoryByMonth;

public record AskStatisticSubCategoryEvent : INotification
{
    public long SessionId { get; init; }
    public int LastSentMessageId { get; init; }
    public IReadOnlyList<SubCategory> SubCategories { get; init; }
}