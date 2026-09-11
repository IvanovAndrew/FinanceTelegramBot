using Application.Core.Services;
using Domain;
using MediatR;

namespace Application.Api;

public record GetBalanceStatisticApiCommand : IRequest<BalanceStatisticResult>
{
    public YearMonth StartDate { get; init; }
    public Currency Currency { get; init; }
}

public class GetBalanceStatisticApiCommandHandler(IBalanceStatisticService balanceService)
    : IRequestHandler<GetBalanceStatisticApiCommand, BalanceStatisticResult>
{
    public async Task<BalanceStatisticResult> Handle(GetBalanceStatisticApiCommand request,
        CancellationToken cancellationToken)
    {
        return await balanceService.Calculate(request.StartDate, request.Currency, cancellationToken);
    }
}