using MediatR;

namespace Application.Commands;

public class CancelSessionCommand : IRequest
{
    public long SessionId { get; init; }
}