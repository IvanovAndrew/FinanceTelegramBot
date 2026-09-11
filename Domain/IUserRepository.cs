namespace Domain;

public interface IUserRepository
{
    Task<List<AuthenticatedUser>> ReadAuthenticatedUsers(CancellationToken cancellationToken);
}