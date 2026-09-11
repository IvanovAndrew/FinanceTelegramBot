using Application.Contracts;
using Application.Core;
using Domain;
using Infrastructure.Fns;

namespace Infrastructure.YerevanCity;

public class FnsReceiptProvider(IFnsAPIService fnsApiService, RussianCheckExpenseJsonParser parser) : IFnsReceiptProvider
{
    public async Task<Result<Check>> DownloadCheck(CheckRequisite requisite, CancellationToken cancellationToken)
    {
        var check = await fnsApiService.GetCheck(requisite, cancellationToken);
        
        if (check is {Success: false, ErrorMessage: var errorMessage})
        {
            return Result<Check>.Failure(errorMessage);
        }

        return Result<Check>.Success(parser.ParseCheck(check.Data));
    }
}