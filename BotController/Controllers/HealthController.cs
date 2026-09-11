using Application.Contracts;
using Application.Core;
using Application.Core.Services;
using Infrastructure.Fns;
using Infrastructure.GoogleSpreadsheet;
using Microsoft.AspNetCore.Mvc;

namespace TelegramBot.Controllers;

[ApiController]
[Route("api")]
public class HealthController(IGoogleSpreadsheetApi googleSpreadsheetApi, IYerevanCityAPI yerevanCityApi, IFnsAPIService fnsApiService, IAdminNotificationService adminNotificationService, ILogger<HealthController> logger) : ControllerBase
{
    [HttpGet("health")]
    public async Task<Dictionary<string, bool>> Health(CancellationToken cancellationToken)
    {
        var dict = new Dictionary<string, bool>();
        
        try
        {
            dict["GoogleSpreadsheet"] = await IsGoogleSpreadsheetHealth(cancellationToken);
            dict["Yerevan city API"] = await IsYerevanCityAPIAvailable(cancellationToken);
            dict["FNS API"] = await IsFNSApiAvailable(cancellationToken);
            
            return dict;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Health check failed");
            await adminNotificationService.NotifyCrash(e, cancellationToken);
            return new Dictionary<string, bool>();
        }
    }

    private async Task<bool> IsGoogleSpreadsheetHealth(CancellationToken cancellationToken)
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
    
    private async Task<bool> IsYerevanCityAPIAvailable(CancellationToken cancellationToken)
    {
        try
        {
            var response = await yerevanCityApi.DownloadRawJson(DateOnlyUtils.Today, "LN0000000000", cancellationToken);
            
            return response != null;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Health check failed");
            await adminNotificationService.NotifyCrash(e, cancellationToken);
            return false;
        }
    }
    
    private async Task<bool> IsFNSApiAvailable(CancellationToken cancellationToken)
    {
        try
        {
            var response = await fnsApiService.GetCheck(new CheckRequisite(), cancellationToken);
            
            return response.ErrorMessage != "Unknown error";
        }
        catch (Exception e)
        {
            logger.LogError(e, "FNS Health check failed");
            await adminNotificationService.NotifyCrash(e, cancellationToken);
            return false;
        }
    }
}