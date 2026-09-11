using Domain;
using MediatR;

namespace Application.Api;

public record GetOutcomeApiCommand : IRequest<IReadOnlyDictionary<DateOnly, IReadOnlyList<ShopExpenses>>>
{
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public Currency Currency { get; init; }
}