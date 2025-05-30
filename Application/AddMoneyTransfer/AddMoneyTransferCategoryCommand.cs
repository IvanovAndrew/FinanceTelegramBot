using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.AddMoneyTransfer;

public class AddMoneyTransferCategoryCommand : IRequest
{
    public long SessionId { get; init; }
    public string Category { get; init; }
}

public class AddExpenseCategoryCommandHandler(
    IMediator mediator,
    IUserSessionService userSessionService,
    ICategoryProvider categoryProvider, ILogger<AddExpenseCategoryCommandHandler> logger)
    : IRequestHandler<AddMoneyTransferCategoryCommand>
{
    public async Task Handle(AddMoneyTransferCategoryCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation($"{nameof(AddExpenseCategoryCommandHandler)} started");
        
        var session = userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            var category = categoryProvider.GetCategories(session.MoneyTransferBuilder.IsIncome).FirstOrDefault(c =>
                string.Equals(c.ShortName, request.Category, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(c.Name, request.Category, StringComparison.InvariantCultureIgnoreCase));

            if (category != null)
            {
                session.MoneyTransferBuilder.Category = category;
                session.QuestionnaireService.Next();
                
                logger.LogInformation($"Category {category?.Name} saved");

                await mediator.Publish(new OutcomeCategoryEnteredEvent(){SessionId = request.SessionId, LastSentMessageId = session.LastSentMessageId}, cancellationToken);
            }
            else
            {
                logger.LogWarning($"Category {request.Category} hasn't been found");
            }
        }
        
        logger.LogInformation($"{nameof(AddExpenseCategoryCommandHandler)} finished");
    }
}