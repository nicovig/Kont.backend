namespace Kont.backend.Models.Dashboard;

public class RecentActivity
{
    public string Id { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Message { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public string? PoolId { get; set; }
    public string? PlayerId { get; set; }
}
