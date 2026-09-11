using Domain;
using Microsoft.Extensions.Logging;

namespace Application.Core.Services;

public interface IAuthenticationService
{
    Task<bool> IsUserAuthorized(long chatId, CancellationToken cancellationToken);
    Task<AuthenticatedUser?> GetUser(long chatId, CancellationToken cancellationToken);
    Task<List<AuthenticatedUser>> GetAdmins(CancellationToken cancellationToken);
}

public class AuthenticationService(IUserRepository userRepository, ILogger<IAuthenticationService> logger) : IAuthenticationService
{
    private List<AuthenticatedUser>? _cachedUsers;

    public async Task<bool> IsUserAuthorized(long chatId, CancellationToken cancellationToken)
    {
        var user = await GetUser(chatId, cancellationToken);
        return user != null;
    }

    public async Task<AuthenticatedUser?> GetUser(long chatId, CancellationToken cancellationToken)
    {
        var users = await GetAuthorizedUsers(cancellationToken);
        return users.FirstOrDefault(u => u.TelegramChatId == chatId);
    }

    public async Task<List<AuthenticatedUser>> GetAdmins(CancellationToken cancellationToken)
    {
        var users = await GetAuthorizedUsers(cancellationToken);
        return users.Where(u => u.IsAdmin).ToList();
    }

    private async Task<List<AuthenticatedUser>> GetAuthorizedUsers(CancellationToken cancellationToken)
    {
        _cachedUsers ??= await userRepository.ReadAuthenticatedUsers(cancellationToken);
        
        return _cachedUsers;
    }
}
