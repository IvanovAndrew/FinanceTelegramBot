namespace Application;

public interface IUserSessionService
{
    public UserSession? GetUserSession(long sessionId);
    public void SaveUserSession(UserSession userSession);
    void RemoveSession(UserSession userSession) => RemoveSession(userSession.Id);
    void RemoveSession(long sessionId);
}