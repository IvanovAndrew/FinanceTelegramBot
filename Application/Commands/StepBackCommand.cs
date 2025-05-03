using MediatR;

namespace Application. Commands;

public class StepBackCommand : IRequest
{
    public long SessionId { get; init; }
}