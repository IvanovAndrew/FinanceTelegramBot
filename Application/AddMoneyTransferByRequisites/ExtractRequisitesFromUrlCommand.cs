using Application.Contracts;
using MediatR;

namespace Application.AddMoneyTransferByRequisites;

public record ExtractRequisitesFromUrlCommand : IRequest
{
    public long SessionId { get; init; }
    public string Url { get; init; }
}

public class ExtractRequisitesFromUrlCommandHandler(IMediator mediator) : IRequestHandler<ExtractRequisitesFromUrlCommand>
{
    public async Task Handle(ExtractRequisitesFromUrlCommand request, CancellationToken cancellationToken)
    {
        var checkRequisite = CheckRequisite.FromUrlLink(request.Url);

        await mediator.Send(new DownloadExpenseFromFNSServiceCommand()
            { SessionId = request.SessionId, CheckRequisite = checkRequisite }, cancellationToken);
    }
}