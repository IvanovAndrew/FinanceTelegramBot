namespace Application;

public interface IUserSessionService
{
    public UserSession? GetUserSession(long chatId);
    public void SaveUserSession(UserSession userSession);
}