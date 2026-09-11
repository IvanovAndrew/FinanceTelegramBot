using Application.Core.Commands;
using MediatR;

namespace Application.Bot;

public class StepBackCommandHandler(IUserSessionService userSessionService) : IRequestHandler<StepBackCommand>
{
    public Task Handle(StepBackCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}