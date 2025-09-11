using Kont.backend.DAL;

namespace Kont.backend.Models.User;

public class AuthResponse
{
    public string Token { get; set; } = null!;
    public Administrator Administrator { get; set; } = null!;
}
