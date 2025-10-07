namespace Kont.backend.Models.Request;

public class PlayerRegisterRequest
{
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Pin { get; set; } = string.Empty;
}


