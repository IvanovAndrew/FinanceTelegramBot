using Domain;

namespace Infrastructure;

public class UserSettings
{
    public List<long> Authenticated { get; init; } = new List<long>();
    public List<long> Admins { get; init; } = new List<long>();
}

public class UserRepository : IUserRepository
{
    private readonly UserSettings _users;

    public UserRepository(UserSettings users)
    {
        _users = users;
    }
    
    public Task<List<AuthenticatedUser>> ReadAuthenticatedUsers(CancellationToken cancellationToken)
    {
        return Task.FromResult(_users.Authenticated.Select(x => new AuthenticatedUser(x, _users.Admins.Contains(x))).ToList());
    }
}