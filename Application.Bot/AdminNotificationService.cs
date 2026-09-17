using Application.Core;
using Application.Core.Services;
using Microsoft.Extensions.Logging;

namespace Application.Bot;

public class AdminNotificationService(
    IMessageService messageService,
    IAuthenticationService authenticationService,
    ILogger<AdminNotificationService> logger) : IAdminNotificationService
{
    public async Task NotifyUnauthorizedAccessAsync(long unauthorizedChatId, CancellationToken cancellationToken)
    {
        await Notify($"⚠️ Unauthorized access attempt from ChatID: {unauthorizedChatId}", cancellationToken);
    }

    public async Task NotifyNewProductCodes(IReadOnlyList<NewOption> unknownOptions, CancellationToken cancellationToken)
    {
        var unknownOptionsString = string.Join(", ", unknownOptions.Select(x => $"{x.Code} - {x.Description}"));
        var notificationText = $"New product codes are available:{Environment.NewLine}{unknownOptionsString}";
        
        await Notify(notificationText, cancellationToken);
    }

    public async Task NotifyCrash(Exception exception, CancellationToken cancellationToken)
    {
        var text = $"⚠️ Unhandled exception: {exception}";
        await Notify(text, cancellationToken);
    }

    public async Task NotifyCategoryIssues(List<CategoryResolutionIssueBase> issues, CancellationToken cancellationToken)
    {
        var notificationText = $"There are issues with the following categories:{Environment.NewLine} {string.Join(Environment.NewLine, issues.Select(i => i.ToString()))}";
        await Notify(notificationText, cancellationToken);
    }

    private async Task Notify(string text, CancellationToken cancellationToken)
    {
        try
        {
            var admins = await authenticationService.GetAdmins(cancellationToken);

            foreach (var admin in admins)
            {
                try
                {
                    await messageService.SendTextMessageAsync(
                        admin.TelegramChatId,
                        text,
                        cancellationToken: cancellationToken);

                    logger.LogInformation(text);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to send notification to admin {AdminChatId}", admin.TelegramChatId);
                }
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, $"Error notifying admins about the error {exception}");
        }
    }
}