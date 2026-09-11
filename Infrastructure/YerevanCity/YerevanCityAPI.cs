using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Contracts;

namespace Infrastructure.YerevanCity;

public class YerevanCityAPI : IYerevanCityAPI
{
    private readonly HttpClient _httpClient;

    public YerevanCityAPI(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> DownloadRawJson(DateOnly date, string code, CancellationToken cancellationToken)
    {
        const string url = "https://apishopv2.yerevan-city.am/api/Order/GetOfflineOrderById";

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzNTNlMzIyMC1kOGYwLTQ5Y2EtODIxZS0yODBhNDg5NDA4NDciLCJ1bmlxdWVfbmFtZSI6IiszNzQ0NDI0MjIwNSIsImp0aSI6IjI4ZjNhZDUxLTBjYTktNDdjOC05MGJiLTNjNTQ4YzI0NGQ1YyIsImlhdCI6MTc4MDQ5MTA4OSwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiVXNlciIsIm5iZiI6MTc4MDQ5MTA4OSwiZXhwIjoxNzg5MTMxMDg5LCJpc3MiOiJ3ZWJBcGkiLCJhdWQiOiJodHRwOi8vbG9jYWxob3N0OjUwMDIvIn0.fxeQbAdNAR2Rc5TP8uBNlGdQyP809htKkwUEWIW0QwM");
        request.Headers.Add("cityid", "10057");
        request.Headers.Add("ostype", "3");
        
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var payload = new
        {
            orderId = code,
            createdOn = date.ToString("yyyy-MM-dd")
        };

        request.Content = JsonContent.Create(payload);
        request.Content.Headers.Add("content-language", "2");

        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new InternalExternalApiException(ex);
        }

        return null;
    }
}