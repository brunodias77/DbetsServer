using Dbets.Domain.Aggregates;

namespace Dbets.Domain.Services;

public interface ILoggedUser
{
    public Task<User> User();
}