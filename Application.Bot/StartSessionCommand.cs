using MediatR;

namespace Application.Core.Commands;

public record StartSessionCommand : IRequest
{
    public long SessionId { get; init; }
}