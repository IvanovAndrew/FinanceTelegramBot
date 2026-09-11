using Application.Contracts;
using Application.Core;
using Application.Core.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Bot;

public record DownloadExpensesFromFNSServiceCommand : IRequest
{
    public long SessionId { get; init; }
    public DateOnly Today { get; init; }
    public CheckRequisite CheckRequisite { get; init; }
}

public class DownloadExpenseFromFNSServiceCommandHandler(IUserSessionService userSessionService, IFnsReceiptProvider fnsReceiptProvider, IExpenseCategorizer expenseCategorizer, IExpenseCategoryMappingCache expenseCategoryMappingCache, IExternalCategoryMapper externalCategoryMapper, IAdminNotificationService adminNotificationService, IConversation conversationUi, IMediator mediator, ILogger<DownloadExpenseFromFNSServiceCommandHandler> logger) : IRequestHandler<DownloadExpensesFromFNSServiceCommand>
{
    public async Task Handle(DownloadExpensesFromFNSServiceCommand request, CancellationToken cancellationToken)
    {
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            // await conversationUi.Update(request.SessionId, Screens.NotifyOperationInProgress("Loading category mapping..."), cancellationToken);
            
            // var mapping = await expenseCategoryMappingCache.Get(Currency.RUR, request.Today.AddYears(-1), cancellationToken);
            
            await conversationUi.Update(request.SessionId, Screens.NotifyOperationInProgress("Downloading the outcomes from FNS service"), cancellationToken);
        
            var downloadResult = await fnsReceiptProvider.DownloadCheck(request.CheckRequisite, cancellationToken);
            
            if (downloadResult is {IsSuccess:false, Error: var error})
            {
                logger.LogError(error, "Error while downloading expenses from FNS service");
                await conversationUi.Update(request.SessionId, Screens.Notify(error), cancellationToken);
                return;
            }

            var check = downloadResult.Value;
            
            logger.LogInformation("Expenses are successfully downloaded from FNS service. Check.Available options count: {AvailableOptionsCount}, Check.Outcomes count: {OutcomesCount}", check.NewOptions.Count, check.Outcomes.Count);
            
            await conversationUi.Update(request.SessionId, Screens.NotifyOperationInProgress("Expenses are successfully downloaded from FNS service"), cancellationToken);
            await mediator.Send(new SaveOutcomesBatchCommand() { SessionId = session.Id, MoneyTransfers = check.Outcomes }, cancellationToken);
            
            if (check.NewOptions.Any())
            {
                await adminNotificationService.NotifyNewProductCodes(check.NewOptions, cancellationToken);
            }
        }
    }
}