using Application;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure;

public class UserSessionService : IUserSessionService
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions(){ExpirationScanFrequency = TimeSpan.FromMinutes(30)});
    
    public UserSession GetUserSession(long sessionId)
    {
        if (_cache.TryGetValue(sessionId, out UserSession userSession))
        {
            return userSession;
        }

        return null;
    }

    public void SaveUserSession(UserSession userSession)
    {
        _cache.Set(userSession.Id, userSession);
    }

    public void RemoveSession(long sessionId)
    {
        _cache.Remove(sessionId);
    }
}