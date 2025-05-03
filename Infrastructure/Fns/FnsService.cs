using Application;
using Domain;
using Infrastructure.Fns.DataContract;
using Newtonsoft.Json;

namespace Infrastructure.Fns;

public class FnsService : IFnsService
{
    private const string Url = "https://proverkacheka.com/api/v1/check/get";
    private readonly string _token;

    public FnsService(string token)
    {
        _token = !string.IsNullOrEmpty(token)? token : throw new WrongConfigurationFnsException(nameof(token));
    }
    
    public async Task<IReadOnlyCollection<Outcome>> GetCheck(string qrRaw)
    {
        using HttpClient client = new HttpClient();
        
        using HttpContent content = new FormUrlEncodedContent(
            new Dictionary<string, string>()
            {
                ["qrraw"] = qrRaw,
                ["token"] = _token
            });

        var response = await client.PostAsync(Url, content);

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            try
            {
                var fnsResponse = JsonConvert.DeserializeObject<FnsResponse>(responseString);

                var json = fnsResponse.Data?.Json;
                
                var expenses = json?.Items?.Select(i => new Outcome()
                {
                    Amount = new Money
                    {
                        Amount = i.Sum / 100m,
                        Currency = Currency.Rur
                    },
                    Date = GetDate(json.DateTime),
                    Description = i.Name,
                    Category = "Еда"
                })?.ToList()?? new List<Outcome>();

                return expenses;
            }
            catch (Exception e)
            {
                var error = JsonConvert.DeserializeObject<FnsErrorResponse>(responseString);
                throw new FnsException(error.Data);
            }
        }

        return [];
        
        DateOnly GetDate(DateTime date)
        {
            return DateOnly.FromDateTime(date.Hour < 4? date.AddDays(-1) : date);
        }
    }
}