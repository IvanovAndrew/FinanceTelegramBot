using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.Contracts;

namespace Infrastructure.YerevanCity;

public class YerevanCityAPI(HttpClient httpClient, string authParameter) : IYerevanCityAPI
{
    public async Task<string?> DownloadRawJson(DateOnly date, string code, CancellationToken cancellationToken)
    {
        const string url = "https://apishopv2.yerevan-city.am/api/Order/GetOfflineOrderById";

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authParameter);
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
            var response = await httpClient.SendAsync(request, cancellationToken);
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