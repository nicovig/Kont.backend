namespace Kont.backend.Models.Email;

public class EventInvitationEmailModel
{
    public required string EventName { get; set; }
    public required string PoolName { get; set; }
    public required string SiteName { get; set; }
    public required DateTime EventDate { get; set; }
    public required string JoinUrl { get; set; }
    public required string QrCodeBase64 { get; set; }
    public required string Locale { get; set; }
}
