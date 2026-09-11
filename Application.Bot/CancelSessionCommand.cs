using MediatR;

namespace Application.Core.Commands;

public record CancelSessionCommand : IRequest
{
    public long SessionId { get; init; }
}