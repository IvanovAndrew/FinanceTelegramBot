using MediatR;

namespace Application.Commands;

public class StartSessionCommand : IRequest
{
    public long SessionId { get; init; }
}