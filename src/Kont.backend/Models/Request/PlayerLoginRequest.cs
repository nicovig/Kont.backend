namespace Kont.backend.Models.Request;

public class PlayerLoginRequest
{
    public string Identifier { get; set; } = string.Empty; // email or username
    public string Pin { get; set; } = string.Empty;
}