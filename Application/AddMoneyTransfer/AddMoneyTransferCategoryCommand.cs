using MediatR;

namespace Application.AddMoneyTransfer;

public class AddMoneyTransferCategoryCommand : IRequest
{
    public long SessionId { get; init; }
    public string Category { get; init; }
}

public class AddExpenseCategoryCommandHandler(
    IMediator mediator,
    IUserSessionService userSessionService,
    ICategoryProvider categoryProvider)
    : IRequestHandler<AddMoneyTransferCategoryCommand>
{
    public async Task Handle(AddMoneyTransferCategoryCommand request, CancellationToken cancellationToken)
    {
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

                await mediator.Publish(new OutcomeCategoryEnteredEvent(){SessionId = request.SessionId, LastSentMessageId = session.LastSentMessageId}, cancellationToken);
            }
        }
    }
}