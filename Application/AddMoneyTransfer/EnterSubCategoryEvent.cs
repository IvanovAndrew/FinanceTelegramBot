using Domain;
using MediatR;

namespace Application.AddMoneyTransfer;

public record EnterSubCategoryEvent : INotification
{
    public long SessionId { get; init; }
    public IReadOnlyList<SubCategory> SubCategories { get; init; }
}