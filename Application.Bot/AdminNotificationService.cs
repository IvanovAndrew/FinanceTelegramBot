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
        try
        {
            var admins = await authenticationService.GetAdmins(cancellationToken);

            if (admins.Count == 0)
            {
                logger.LogWarning("No admins found to notify about unauthorized access from {ChatId}",
                    unauthorizedChatId);
                return;
            }

            var notificationText = $"⚠️ Unauthorized access attempt from ChatID: {unauthorizedChatId}";

            foreach (var admin in admins)
            {
                try
                {
                    await messageService.SendTextMessageAsync(
                        admin.TelegramChatId,
                        notificationText,
                        cancellationToken: cancellationToken);

                    logger.LogInformation(
                        "Notified admin {AdminChatId} about unauthorized access from {ChatId}",
                        admin.TelegramChatId, unauthorizedChatId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to send notification to admin {AdminChatId}", admin.TelegramChatId);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error notifying admins about unauthorized access from {ChatId}", unauthorizedChatId);
        }
    }

    public async Task NotifyNewProductCodes(HashSet<string> unknownOptions, CancellationToken cancellationToken)
    {
        try
        {
            var admins = await authenticationService.GetAdmins(cancellationToken);

            if (admins.Count == 0)
            {
                logger.LogWarning("No admins found to notify about new product codes: {ProductCodes}", string.Join(", ", unknownOptions));
                return;
            }

            var notificationText = $"New product codes are available:{Environment.NewLine}{string.Join($"{Environment.NewLine}", unknownOptions)}";

            foreach (var admin in admins)
            {
                try
                {
                    await messageService.SendTextMessageAsync(
                        admin.TelegramChatId,
                        notificationText,
                        cancellationToken: cancellationToken);

                    logger.LogInformation(
                        "Notified admin {AdminChatId} about new product codes: {ProductCodes}",
                        admin.TelegramChatId, string.Join(", ", unknownOptions));
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to send notification to admin {AdminChatId}", admin.TelegramChatId);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error notifying admins about new product codes: {ProductCodes}", string.Join(", ", unknownOptions));
        }
    }

    public async Task NotifyCrash(Exception exception, CancellationToken cancellationToken)
    {
        try
        {
            var admins = await authenticationService.GetAdmins(cancellationToken);

            var notificationText = $"⚠️ Unhandled exception: {exception.ToString()}";

            foreach (var admin in admins)
            {
                try
                {
                    await messageService.SendTextMessageAsync(
                        admin.TelegramChatId,
                        notificationText,
                        cancellationToken: cancellationToken);

                    logger.LogInformation(notificationText);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to send notification to admin {AdminChatId}", admin.TelegramChatId);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error notifying admins about the error {exception}");
        }
    }
}