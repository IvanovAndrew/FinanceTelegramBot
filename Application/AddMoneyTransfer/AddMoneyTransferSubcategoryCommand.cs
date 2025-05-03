using MediatR;

namespace Application.AddMoneyTransfer;

public class AddMoneyTransferSubcategoryCommand : IRequest
{
    public long SessionId { get; init; }
    public string Subcategory { get; init; }
}

public class AddExpenseSubcategoryCommandHandler(IUserSessionService userSessionService, IMediator mediator) : IRequestHandler<AddMoneyTransferSubcategoryCommand>
{
    public async Task Handle(AddMoneyTransferSubcategoryCommand request, CancellationToken cancellationToken)
    {
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

                await mediator.Publish(new OutcomeSubCategoryEnteredEvent(){SessionId = request.SessionId}, cancellationToken);
            }
        }
    }
}

public class OutcomeSubCategoryEnteredEvent : INotification
{
    public long SessionId { get; init; }
}