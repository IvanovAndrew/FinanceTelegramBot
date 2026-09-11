using Application.Core;
using Domain;
using MediatR;

namespace Application.Api;

public class GetSpendingDailyHistoryApiCommandHandler(IFinanceRepository financeRepository) : IRequestHandler<GetSpendingDailyHistoryApiCommand, SpendingDailyHistoryResult>
{
    public async Task<SpendingDailyHistoryResult> Handle(GetSpendingDailyHistoryApiCommand request, CancellationToken cancellationToken)
    {
        var currency = request.Currency;
        var dateRange = new DateRange(request.From, request.To);

        var filter = new FinanceFilter { Currency = currency, DateFrom = request.From };
        var outcomes = await financeRepository.ReadOutcomes(filter, cancellationToken);

        var outcomesByDay = outcomes.ToLookup(o => o.Date);

        var days = new List<DaySpending>();
        
        for (var day = dateRange.From; day <= dateRange.To; day = day.AddDays(1))
        {
            var dayOutcomes = outcomesByDay[day];

            var shops = dayOutcomes
                .GroupBy(o => o.Shop)
                .Select(shopGroup =>
                {
                    var categories = shopGroup
                        .GroupBy(o => o.Category)
                        .Select(categoryGroup =>
                        {
                            var subCategories = categoryGroup
                                .GroupBy(o => o.SubCategory)
                                .Select(subCategoryGroup =>
                                {
                                    var subCategoryTotal = GetSpendingHistoryApiCommandHandler.Sum(
                                        subCategoryGroup.Select(o => o.Amount), currency);

                                    return new SubCategorySpending(subCategoryGroup.Key, subCategoryTotal);
                                })
                                .ToList();

                            var categoryTotal = GetSpendingHistoryApiCommandHandler.Sum(
                                subCategories.Select(sc => sc.Total), currency);

                            return new CategorySpending(categoryGroup.Key, categoryTotal, subCategories);
                        })
                        .ToList();

                    return new ShopSpending(shopGroup.Key, categories);
                })
                .ToList();

            var dayTotal = GetSpendingHistoryApiCommandHandler.Sum(
                dayOutcomes.Select(o => o.Amount), currency);

            days.Add(new DaySpending(day, dayTotal, shops));
        }

        return new SpendingDailyHistoryResult(currency, days);
    }
}