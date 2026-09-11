using MediatR;

namespace Application.Core. Commands;

public record StepBackCommand : IRequest
{
    public long SessionId { get; init; }
}