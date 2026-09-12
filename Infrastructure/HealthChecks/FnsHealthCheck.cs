using Application.Contracts;
using Infrastructure.Fns;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Infrastructure.HealthChecks;

public class FnsHealthCheck(IFnsAPIService fnsApiService)  : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            var result = await fnsApiService.GetCheck(new CheckRequisite(), cancellationToken);
            var status = result.ErrorMessage != "Unknown error";

            return new HealthCheckResult(status ? HealthStatus.Healthy : HealthStatus.Unhealthy);
        }
        catch (Exception e)
        {
            return new HealthCheckResult(HealthStatus.Unhealthy);
        }
    }
}