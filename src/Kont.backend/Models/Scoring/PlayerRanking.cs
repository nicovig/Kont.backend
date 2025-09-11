namespace Kont.backend.Models.Scoring;

public class PlayerRanking
{
    public Guid PlayerId { get; set; }
    public string PlayerName { get; set; } = null!;
    public string Username { get; set; } = null!;
    public double GlobalScore { get; set; }
    public double GlobalPercentage { get; set; }
    public int GlobalRank { get; set; }
    public int ActivitiesPlayed { get; set; }
    public List<ActivityScore> ActivityScores { get; set; } = new List<ActivityScore>();
}

public class ActivityScore
{
    public Guid ActivityId { get; set; }
    public string ActivityName { get; set; } = null!;
    public double Score { get; set; }
    public double Percentage { get; set; }
    public int Rank { get; set; }
    public List<PlayerMetricResult> MetricScores { get; set; } = new List<PlayerMetricResult>();
}

public class PlayerMetricResult
{
    public Guid MetricId { get; set; }
    public string MetricName { get; set; } = null!;
    public string? Unit { get; set; }
    public double Value { get; set; }
    public double NormalizedValue { get; set; }
    public double Coefficient { get; set; }
    public bool HigherIsBetter { get; set; }
}
