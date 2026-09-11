using Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Api;

public class GetIncomeApiHandler(IFinanceRepository financeRepository, ILogger<GetIncomeApiHandler> logger) : IRequestHandler<GetIncomeApiCommand, IReadOnlyList<Income>>
{
    public async Task<IReadOnlyList<Income>> Handle(GetIncomeApiCommand request, CancellationToken cancellationToken)
    {
        return await financeRepository.ReadIncomes(new FinanceFilter(){DateFrom = request.Day, DateTo = request.Day, Currency = request.Currency}, cancellationToken);
    }
}