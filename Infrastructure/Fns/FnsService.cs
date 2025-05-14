using Application;
using Domain;
using Infrastructure.Fns.DataContract;
using Newtonsoft.Json;
using Refit;

namespace Infrastructure.Fns;

public class FnsService(IFnsApi api, ICategoryProvider categoryProvider, string token) : IFnsService
{
    private const string Url = "https://proverkacheka.com/api/v1/check/get";
    private readonly string _token = !string.IsNullOrEmpty(token)? token : throw new WrongConfigurationFnsException(nameof(token));

    public async Task<IReadOnlyCollection<Outcome>> GetCheck(CheckRequisite checkRequisite)
    {
        try
        {
            var response = await api.GetCheck(new Dictionary<string, string>
            {
                ["qrraw"] = checkRequisite.ToQueryString(),
                ["token"] = _token
            });
        
            var json = response.Data?.Json;
            if (json?.Items == null) return [];

            var defaultCategory = categoryProvider.DefaultOutcomeCategory();
        
            var expenses = json?.Items?.Select(i => new Outcome()
            {
                Amount = new Money
                {
                    Amount = i.Sum / 100m,
                    Currency = Currency.Rur
                },
                Date = GetDate(json.DateTime),
                Description = i.Name,
                Category = defaultCategory
            })?.ToList()?? new List<Outcome>();

            return expenses;
        }
        catch (ApiException ex)
        {
            var errorContent = ex.Content ?? string.Empty;

            FnsErrorResponse? error = null;

            try
            {
                error = JsonConvert.DeserializeObject<FnsErrorResponse>(errorContent);
            }
            catch (JsonException)
            {
                throw new FnsException("Invalid error response format");
            }

            throw new FnsException(error?.Data ?? "Unknown error");
        }
        
        DateOnly GetDate(DateTime date)
        {
            return DateOnly.FromDateTime(date.Hour < 4? date.AddDays(-1) : date);
        }
    }
}