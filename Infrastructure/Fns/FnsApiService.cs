using System.Text.Json;
using Application.Contracts;
using Application.Contracts.FNS;
using Application.Core;
using Domain;
using Microsoft.Extensions.Logging;
using Refit;

namespace Infrastructure.Fns;

public class FnsApiService(IFnsApi api, string token, ILogger<FnsApiService> logger) : IFnsAPIService
{
    private readonly string _token = !string.IsNullOrEmpty(token)? token : throw new WrongConfigurationFnsException(nameof(token));

    public async Task<SaveResult<FnsCheckInfo>> GetCheck(CheckRequisite checkRequisite, CancellationToken cancellationToken)
    {
        try
        {
            var response = await api.GetCheck(new Dictionary<string, string>
            {
                ["qrraw"] = checkRequisite.ToQueryString(),
                ["token"] = _token
            }, cancellationToken);
        
            var json = response.Data?.Json;
            if (json?.Items == null) return SaveResult<FnsCheckInfo>.Fail("No items found");
            
            return SaveResult<FnsCheckInfo>.Ok(json);
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, $"Error while getting check from FNS API {ex.Content}");
            var errorContent = ex.Content ?? string.Empty;

            FnsErrorResponse? error = null;

            try
            {
                error = JsonSerializer.Deserialize<FnsErrorResponse>(errorContent);
            }
            catch (JsonException e)
            {
                throw new FnsException("Invalid error response format");
            }

            return SaveResult<FnsCheckInfo>.Fail(error?.Data ?? "Unknown error");
        }
    }
}