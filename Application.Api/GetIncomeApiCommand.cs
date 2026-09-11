using Domain;
using MediatR;

namespace Application.Api;

public record GetIncomeApiCommand : IRequest<IReadOnlyList<Income>>
{
    public DateOnly Day { get; init; }
    public Currency Currency { get; init; }
}