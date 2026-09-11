using Application.Core.Services;
using Infrastructure.GoogleSpreadsheet;
using Microsoft.AspNetCore.Mvc;

namespace TelegramBot.Controllers;

[ApiController]
[Route("api")]
public class HealthController(IGoogleSpreadsheetApi googleSpreadsheetApi, IAdminNotificationService adminNotificationService, ILogger<HealthController> logger) : ControllerBase
{
    [HttpGet("health")]
    public async Task<bool> Health(CancellationToken cancellationToken)
    {
        try
        {
            return await googleSpreadsheetApi.HealthCheck(cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Health check failed");
            await adminNotificationService.NotifyCrash(e, cancellationToken);
            return false;
        }
    }
}