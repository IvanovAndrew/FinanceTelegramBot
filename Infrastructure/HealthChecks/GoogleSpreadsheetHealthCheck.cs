using Infrastructure.GoogleSpreadsheet;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Infrastructure.HealthChecks;

public class GoogleSpreadsheetHealthCheck(IGoogleSpreadsheetApi googleSpreadsheetApi) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        var result = await googleSpreadsheetApi.HealthCheck(cancellationToken);
        
        return result ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
    }
}