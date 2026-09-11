using Domain;
using MediatR;

namespace Application.Api;

public record SaveMoneyTransferApiCommand : IRequest<SaveResult>
{
    public bool IsOutcome { get; init; }
    public DateOnly Date { get; init; }
    public string Category { get; init; }
    public string? SubCategory { get; init; }
    public string? Shop { get; init; }
    public string? Description { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; }
}

public class SaveMoneyTransferApiCommandHandler(IMediator mediator) : IRequestHandler<SaveMoneyTransferApiCommand, SaveResult>
{
    public async Task<SaveResult> Handle(SaveMoneyTransferApiCommand request, CancellationToken cancellationToken)
    {
        if (!Currency.TryParse(request.Currency, out var currency))
            return SaveResult.Fail($"Unknown currency {request.Currency}");

        var category = request.IsOutcome
            ? Categories.Outcome.GetCategory(request.Category)
            : Categories.Income.GetCategory(request.Category);

        if (category == null)
            return SaveResult.Fail($"Unknown category '{request.Category}'");

        IMoneyTransfer moneyTransfer;
        
        if (request.IsOutcome)
        {
            moneyTransfer = new Outcome()
            {
                Date = request.Date,
                Amount = new Money { Currency = currency, Amount = request.Amount },
                Category = category,
                SubCategory = category.GetSubcategoryByName(request.SubCategory),
                Shop = Shop.Create(request.Shop),
                Description = request.Description
            };
        }
        else
        {
            moneyTransfer = new Income()
            {
                Date = request.Date,
                Amount = new Money { Currency = currency, Amount = request.Amount },
                Category = category,
                Description = request.Description
            };
        }
        
        return await mediator.Send(new SaveTransferCommand { MoneyTransfer = moneyTransfer }, cancellationToken);
    }
}