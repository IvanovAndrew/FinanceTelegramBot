namespace Application.Core.Services;



public interface IAdminNotificationService
{
    Task NotifyUnauthorizedAccessAsync(long unauthorizedChatId, CancellationToken cancellationToken);
    Task NotifyNewProductCodes(IReadOnlyList<NewOption> unknownOptions, CancellationToken cancellationToken);
    Task NotifyCrash(Exception exception, CancellationToken cancellationToken);
    Task NotifyCategoryIssues(List<CategoryResolutionIssueBase> issues, CancellationToken cancellationToken);
}