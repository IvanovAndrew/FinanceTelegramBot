using Domain;

namespace Application.Test.Stubs;

public class UserRepositoryStub : IUserRepository
{
    public Task<List<AuthenticatedUser>> ReadAuthenticatedUsers(CancellationToken cancellationToken)
    {
        return Task.FromResult(new List<AuthenticatedUser>(){new AuthenticatedUser(0, IsAdmin:false) });
        
    }
}