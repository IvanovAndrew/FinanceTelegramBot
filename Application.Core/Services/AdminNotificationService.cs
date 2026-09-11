namespace Application.Core.Services;

public interface IAdminNotificationService
{
    Task NotifyUnauthorizedAccessAsync(long unauthorizedChatId, CancellationToken cancellationToken);
    Task NotifyNewProductCodes(HashSet<string> unknownOptions, CancellationToken cancellationToken);
    Task NotifyCrash(Exception exception, CancellationToken cancellationToken);
}