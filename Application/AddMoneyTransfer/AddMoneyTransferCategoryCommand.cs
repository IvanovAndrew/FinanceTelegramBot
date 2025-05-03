using Application.Events;
using Domain.Events;
using MediatR;

namespace Application.AddMoneyTransfer;

public class AddMoneyTransferCategoryCommand : IRequest
{
    public long SessionId { get; init; }
    public string Category { get; init; }
    public int LastSentMessageId { get; init; }
}

public class AddExpenseCategoryCommandHandler : IRequestHandler<AddMoneyTransferCategoryCommand>
{
    private readonly IMediator _mediator;
    private readonly IUserSessionService _userSessionService;
    private readonly ICategoryProvider _categoryProvider;

    public AddExpenseCategoryCommandHandler(IMediator mediator, IUserSessionService userSessionService, ICategoryProvider categoryProvider)
    {
        _mediator = mediator;
        _userSessionService = userSessionService;
        _categoryProvider = categoryProvider;
    }
    
    public async Task Handle(AddMoneyTransferCategoryCommand request, CancellationToken cancellationToken)
    {
        var session = _userSessionService.GetUserSession(request.SessionId);

        if (session != null)
        {
            var category = _categoryProvider.GetCategories(session.MoneyTransferBuilder.IsIncome).FirstOrDefault(c =>
                string.Equals(c.ShortName, request.Category, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(c.Name, request.Category, StringComparison.InvariantCultureIgnoreCase));

            if (category != null)
            {
                session.MoneyTransferBuilder.Category = category;
                session.QuestionnaireService.Next();

                await _mediator.Publish(new OutcomeCategoryEnteredEvent(){SessionId = request.SessionId, LastSentMessageId = request.LastSentMessageId}, cancellationToken);
            }
        }
    }
}