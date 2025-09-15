namespace Kont.backend.Models.Dashboard;

public class DashboardStats
{
    public int TotalPools { get; set; }
    public int ActivePools { get; set; }
    public int TotalPlayers { get; set; }
    public int TotalActivities { get; set; }
    public List<RecentActivity> RecentActivity { get; set; } = new List<RecentActivity>();
}
