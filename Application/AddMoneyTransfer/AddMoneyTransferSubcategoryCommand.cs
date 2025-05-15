using Application.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.AddMoneyTransfer;

public class AddMoneyTransferSubcategoryCommand : IRequest
{
    public long SessionId { get; init; }
    public string Subcategory { get; init; }
}

public class AddExpenseSubcategoryCommandHandler(IUserSessionService userSessionService, IMediator mediator, ILogger<AddExpenseSubcategoryCommandHandler> logger) : IRequestHandler<AddMoneyTransferSubcategoryCommand>
{
    public async Task Handle(AddMoneyTransferSubcategoryCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation($"{nameof(AddExpenseSubcategoryCommandHandler)} started");
        
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            var subCategory = session.MoneyTransferBuilder.Category.Subcategories.FirstOrDefault(c =>
                string.Equals(c.ShortName, request.Subcategory, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(c.Name, request.Subcategory, StringComparison.InvariantCultureIgnoreCase));

            if (subCategory != null)
            {
                session.MoneyTransferBuilder.SubCategory = subCategory;
                session.QuestionnaireService.Next();

                logger.LogInformation($"Subcategory {subCategory.Name} saved");
                
                await mediator.Publish(new OutcomeSubcategoryEnteredEvent() { SessionId = request.SessionId },
                    cancellationToken);
                
                logger.LogInformation($"Subcategory event has been sent");
            }
            else
            {
                logger.LogWarning($"Couldn't find a subcategory {request.Subcategory}");
            }
        }
        else
        {
            logger.LogWarning($"Couldn't find a session with id {request.SessionId}");
        }
        
        logger.LogInformation($"{nameof(AddExpenseSubcategoryCommandHandler)} finished");
    }
}