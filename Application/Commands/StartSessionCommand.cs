using MediatR;

namespace Application.Commands;

public record StartSessionCommand : IRequest
{
    public long SessionId { get; init; }
}