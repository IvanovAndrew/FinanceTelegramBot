using Domain;
using Newtonsoft.Json;

namespace Infrastructure.GoogleSpreadsheet;

public static class FinanceFilterExtensions
{
    public static string ToJsonPayload(this FinanceFilter filter, bool includeSubcategory)
    {
        var payload = new
        {
            DateFrom = filter.DateFrom,
            DateTo = filter.DateTo,
            Category = filter.Category?.Name,
            Currency = filter.Currency?.Name,
            SubCategory = includeSubcategory ? filter.Subcategory?.Name : null
        };

        return JsonConvert.SerializeObject(payload);
    }
}