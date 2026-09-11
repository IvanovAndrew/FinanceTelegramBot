using Domain;
using MediatR;

namespace Application.Api;

public record SaveTransferCommand : IRequest<SaveResult>
{
    public IMoneyTransfer MoneyTransfer { get; init; }
}

public class SaveTransferCommandHandler(IFinanceRepository financeRepository)
    : IRequestHandler<SaveTransferCommand, SaveResult>
{
    public Task<SaveResult> Handle(SaveTransferCommand request, CancellationToken cancellationToken) =>
        financeRepository.Save(request.MoneyTransfer, cancellationToken);
}