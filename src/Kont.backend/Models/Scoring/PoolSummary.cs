namespace Kont.backend.Models.Scoring;

public class PoolSummary
{
    public Guid PoolId { get; set; }
    public string PoolName { get; set; } = null!;
    public int TotalPlayers { get; set; }
    public int TotalActivities { get; set; }
    public int CompletedSessions { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public List<PlayerRanking> PlayerRankings { get; set; } = new List<PlayerRanking>();
    public List<ActivitySummary> ActivitySummaries { get; set; } = new List<ActivitySummary>();
}

public class ActivitySummary
{
    public Guid ActivityId { get; set; }
    public string ActivityName { get; set; } = null!;
    public int PlayersPlayed { get; set; }
    public double BestScore { get; set; }
    public double AverageScore { get; set; }
}
