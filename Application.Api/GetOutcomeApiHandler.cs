using Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Api;

public class GetOutcomeApiHandler(IFinanceRepository financeRepository, ILogger<GetOutcomeApiHandler> logger) : IRequestHandler<GetOutcomeApiCommand, IReadOnlyDictionary<DateOnly, IReadOnlyList<ShopExpenses>>>
{
    public async Task<IReadOnlyDictionary<DateOnly, IReadOnlyList<ShopExpenses>>> Handle(GetOutcomeApiCommand request, CancellationToken cancellationToken)
    {
        var outcomes = await financeRepository.ReadOutcomes(new FinanceFilter(){DateFrom = request.StartDate, DateTo = request.EndDate, Currency = request.Currency}, cancellationToken);

        var result = new Dictionary<DateOnly, IReadOnlyList<ShopExpenses>>();
        
        for (var date = request.StartDate; date <= request.EndDate; date = date.AddDays(1))
        {
            var shopToOutcomes = new Dictionary<Shop, List<Outcome>>();

            foreach (var outcome in outcomes)
            {
                if (outcome.Date != date) continue;

                var shop = outcome.Shop ?? Shop.UnknownShop;
                if (!shopToOutcomes.TryGetValue(shop, out var checkPositions))
                {
                    checkPositions = new List<Outcome>();
                    shopToOutcomes[shop] = checkPositions;
                }

                checkPositions.Add(outcome);
            }
            
            if (shopToOutcomes.Any())
            {
                result[date] = shopToOutcomes
                    .Select(x => new ShopExpenses { Shop = x.Key, Date = date, Outcomes = x.Value }).ToList();
            }
        }

        return result;
    }
}