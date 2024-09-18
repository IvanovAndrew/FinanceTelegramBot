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

    public async Task<FnsResponse?> GetCheck(string qrRaw)
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
                return JsonConvert.DeserializeObject<FnsResponse>(responseString);
            }
            catch (Exception e)
            {
                var error = JsonConvert.DeserializeObject<FnsErrorResponse>(responseString);
                throw new FnsException(error.Data);
            }
        }

        return null;
    }
}