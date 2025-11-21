using MediatR;

namespace Application.Commands;

public record CancelSessionCommand : IRequest
{
    public long SessionId { get; init; }
}