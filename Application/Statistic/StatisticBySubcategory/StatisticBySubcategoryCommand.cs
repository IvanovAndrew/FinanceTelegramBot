using MediatR;

namespace Application.Commands.StatisticBySubcategory;

public class StatisticBySubcategoryCommand : IRequest
{
    public long SessionId { get; init; }
}