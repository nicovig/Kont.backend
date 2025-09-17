using Kont.backend.DAL;

namespace Kont.backend.Services;

public interface IUserContextService
{
    Administrator? GetCurrentUser();
    bool IsUserAuthenticated();
    Guid? GetCurrentUserId();
}
