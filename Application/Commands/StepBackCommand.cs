using MediatR;

namespace Application. Commands;

public record StepBackCommand : IRequest
{
    public long SessionId { get; init; }
}