using Application.Core;
using Application.Core.Services;
using Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Api;

public class SaveYerevanCityReceiptCommand : IRequest<SaveResult<ShopExpenses>>
{
    public DateOnly Date { get; init; }
    public string Barcode { get; init; }
}

public class SaveYerevanCityReceiptCommandHandler(IYerevanCityReceiptProvider yerevanCityReceiptProvider, IFinanceRepository financeRepository, IAdminNotificationService notificationService, ILogger<SaveYerevanCityReceiptCommandHandler> logger) : IRequestHandler<SaveYerevanCityReceiptCommand, SaveResult<ShopExpenses>>
{
    public async Task<SaveResult<ShopExpenses>> Handle(SaveYerevanCityReceiptCommand request, CancellationToken cancellationToken)
    {
        var check = await yerevanCityReceiptProvider.ProcessAsync(request.Date, request.Barcode, cancellationToken);

        if (check.NewOptions.Any())
        {
            await notificationService.NotifyNewProductCodes(check.NewOptions, cancellationToken);
        }
        
        if (!check.Outcomes.Any())
            return SaveResult<ShopExpenses>.Fail("No valid checks found in the provided data.");
        
        var saveResult = await financeRepository.SaveAllOutcomes(check.Outcomes, cancellationToken);

        if (saveResult is { Success : false, ErrorMessage: var error })
        {
            return SaveResult<ShopExpenses>.Fail(error);
        }
        
        var shopExpenses = new ShopExpenses(){Date = check.Outcomes.First().Date, Shop = check.Outcomes.First().Shop, Outcomes = check.Outcomes};

        return SaveResult<ShopExpenses>.Ok(shopExpenses);
    }
}