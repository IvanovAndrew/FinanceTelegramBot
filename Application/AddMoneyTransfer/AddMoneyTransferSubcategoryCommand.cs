using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.AddMoneyTransfer;

public record AddMoneyTransferSubcategoryCommand : IRequest
{
    public long SessionId { get; init; }
    public string Subcategory { get; init; }
}

public class AddMoneyTransferSubcategoryCommandHandler(IUserSessionService userSessionService, IMediator mediator, ILogger<AddMoneyTransferSubcategoryCommandHandler> logger) : IRequestHandler<AddMoneyTransferSubcategoryCommand>
{
    public async Task Handle(AddMoneyTransferSubcategoryCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation($"{nameof(AddMoneyTransferSubcategoryCommandHandler)} started");
            
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session == null)
        {
            logger.LogWarning($"Couldn't find a session with id {request.SessionId}");
        }
        else
        {
            var subCategory = session.MoneyTransferBuilder.Category.GetSubcategoryByName(request.Subcategory);

            if (subCategory == null)
            {
                logger.LogWarning($"Couldn't find a subcategory {request.Subcategory}");
            }
            else
            {
                session.MoneyTransferBuilder.SubCategory = subCategory;
                session.QuestionnaireService.Next();

                logger.LogInformation($"Subcategory {subCategory.Name} saved");

                await mediator.Publish(new OutcomeSubCategoryEnteredEvent() { SessionId = request.SessionId },
                    cancellationToken);
                logger.LogInformation($"Subcategory event has been sent");
            }
        }

        logger.LogInformation($"{nameof(AddMoneyTransferSubcategoryCommandHandler)} finished");
    }
}

public record OutcomeSubCategoryEnteredEvent : INotification
{
    public long SessionId { get; init; }
}