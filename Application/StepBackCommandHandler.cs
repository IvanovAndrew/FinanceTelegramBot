using Application.Commands;
using MediatR;

namespace Application;

public class StepBackCommandHandler(IUserSessionService userSessionService) : IRequestHandler<StepBackCommand>
{
    public Task Handle(StepBackCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}