using Application.Contracts;
using Application.Core;
using Application.Core.Services;
using Domain;
using Domain.Check;
using MediatR;

namespace Application.Api;

public class SaveFnsCheckCommand : IRequest<SaveResult<ShopExpenses>>
{
    public string? Url { get; init; }
    public DateTime? DateTime { get; init; }
    public string? FiscalDocumentNumber { get; init; }
    public string? FiscalDocumentSign { get; init; }
    public string? FiscalNumber { get; init; }
    public decimal? TotalPrice { get; init; }
}

public class SaveFnsCheckCommandHandler(IFnsReceiptProvider fnsReceiptProvider, IAdminNotificationService notificationService, IFinanceRepository financeRepository) : IRequestHandler<SaveFnsCheckCommand, SaveResult<ShopExpenses>>
{
    public async Task<SaveResult<ShopExpenses>> Handle(SaveFnsCheckCommand request, CancellationToken cancellationToken)
    {
        CheckRequisite checkRequisite;
        
        if (request.Url is not null)
        {
            checkRequisite = CheckRequisite.FromUrlLink(request.Url);
        }
        else
        {
            var fiscalDocumentNumber = FiscalDocumentNumber.Create(request.FiscalDocumentNumber ?? string.Empty);
            if (!fiscalDocumentNumber.IsSuccess) return SaveResult<ShopExpenses>.Fail(fiscalDocumentNumber.Error);
            
            var fiscalDocumentSign = FiscalDocumentSign.Create(request.FiscalDocumentSign ?? string.Empty);
            if (!fiscalDocumentSign.IsSuccess) return SaveResult<ShopExpenses>.Fail(fiscalDocumentSign.Error);
            
            var fiscalNumber = FiscalNumber.Create(request.FiscalNumber ?? string.Empty);
            if (!fiscalNumber.IsSuccess) return SaveResult<ShopExpenses>.Fail(fiscalNumber.Error);
            
            checkRequisite = new CheckRequisite
            {
                DateTime = request.DateTime?? default,
                FiscalDocumentNumber = fiscalDocumentNumber.Value,
                FiscalDocumentSign = fiscalDocumentSign.Value,
                FiscalNumber = fiscalNumber.Value,
                TotalPrice = request.TotalPrice?? default
            };
        }

        // var mapping = await expenseCategoryMappingCache.Get(Currency.RUR, DateOnlyUtils.Today.AddYears(-1), cancellationToken);
        
        var downloadingResult = await fnsReceiptProvider.DownloadCheck(checkRequisite, cancellationToken);
        
        if (downloadingResult is {IsSuccess: false, Error: var errorMessage})
        {
            return SaveResult<ShopExpenses>.Fail(errorMessage);
        }
        
        var check = downloadingResult.Value;
        
        if (!check.Outcomes.Any())
            return SaveResult<ShopExpenses>.Fail("No valid checks found in the provided data.");

        if (check.NewOptions.Any())
        {
            await notificationService.NotifyNewProductCodes(check.NewOptions, cancellationToken);
        }
        
        var saveResult = await financeRepository.SaveAllOutcomes(check.Outcomes, cancellationToken);

        if (!saveResult.Success)
        {
            return SaveResult<ShopExpenses>.Fail(saveResult.ErrorMessage); 
        }

        var shopExpenses = new ShopExpenses()
        {
            Date = check.Outcomes.First().Date,
            Shop = check.Outcomes.First().Shop,
            Outcomes = check.Outcomes
        };

        return SaveResult<ShopExpenses>.Ok(shopExpenses);
    }
}