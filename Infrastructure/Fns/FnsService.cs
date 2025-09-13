using Application.Contracts;
using Infrastructure.Fns.DataContract;
using Newtonsoft.Json;
using Refit;

namespace Infrastructure.Fns;

public class FnsService(IFnsApi api, string token) : IFnsService
{
    private const string Url = "https://proverkacheka.com/api/v1/check/get";
    private readonly string _token = !string.IsNullOrEmpty(token)? token : throw new WrongConfigurationFnsException(nameof(token));

    public async Task<IReadOnlyCollection<RawOutcomeItem>> GetCheck(CheckRequisite checkRequisite)
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

            var expenses = new List<RawOutcomeItem>();

            foreach (var item in json?.Items?? [])
            {
                expenses.Add(
                    new RawOutcomeItem()
                    {
                        Amount = item.Sum / 100m,
                        Date = GetDate(json.DateTime),
                        Description = item.Name,
                    });
            }
        
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