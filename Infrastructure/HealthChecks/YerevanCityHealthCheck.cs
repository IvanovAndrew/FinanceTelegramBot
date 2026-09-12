using Application.Contracts;
using Application.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Infrastructure.HealthChecks;

public class YerevanCityHealthCheck(IYerevanCityAPI yerevanCityApi) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        var response = await yerevanCityApi.DownloadRawJson(DateOnlyUtils.Today, "LN0000000000", cancellationToken);
            
        return new HealthCheckResult(response != null ? HealthStatus.Healthy : HealthStatus.Unhealthy);
    }
}