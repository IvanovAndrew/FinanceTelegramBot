using Application.Bot;

namespace Application.Test.Stubs;

public class UserSessionServiceStub : IUserSessionService
{
    private readonly Dictionary<long, UserSession> _sessions = new Dictionary<long, UserSession>();
    
    public UserSession? GetUserSession(long sessionId)
    {
        return _sessions.GetValueOrDefault(sessionId);
    }

    public void SaveUserSession(UserSession userSession)
    {
        _sessions[userSession.Id] = userSession;
    }

    public void RemoveSession(long sessionId)
    {
        _sessions.Remove(sessionId);
    }
}