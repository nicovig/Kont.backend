using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Kont.backend.Models.Scoring;
using Microsoft.EntityFrameworkCore;

namespace Kont.backend.Services;

public interface IScoringService
{
    Task<PoolSummary> GetPoolSummaryAsync(Guid poolId);
    Task<List<PlayerRanking>> GetPoolRankingsAsync(Guid poolId);
    Task<List<PlayerRanking>> GetActivityRankingsAsync(Guid poolId, Guid activityId);
    Task<PlayerRanking> GetPlayerRankingAsync(Guid poolId, Guid playerId);
    Task<double> CalculatePlayerGlobalScoreAsync(Guid poolId, Guid playerId);
}

public class ScoringService : IScoringService
{
    private readonly IDatabaseContext _context;

    public ScoringService(IDatabaseContext context)
    {
        _context = context;
    }

    public async Task<PoolSummary> GetPoolSummaryAsync(Guid poolId)
    {
        var pool = await _context.Pool.FindAsync(poolId);
        if (pool == null) throw new ArgumentException("Pool not found");

        var rankings = await GetPoolRankingsAsync(poolId);
        var activitySummaries = await GetActivitySummariesAsync(poolId);

        return new PoolSummary
        {
            PoolId = pool.Id,
            PoolName = pool.Name,
            TotalPlayers = pool.PlayerRegistrations.Count,
            TotalActivities = pool.GameSessions.Select(gs => gs.Activity.Id).Distinct().Count(),
            CompletedSessions = pool.GameSessions.Count(gs => gs.Status == GameSessionStatus.Completed),
            StartedAt = pool.StartedAt,
            EndedAt = pool.EndedAt,
            PlayerRankings = rankings,
            ActivitySummaries = activitySummaries
        };
    }

    public async Task<List<PlayerRanking>> GetPoolRankingsAsync(Guid poolId)
    {
        // Récupération directe depuis la BDD - plus de calculs !
        var globalScores = await _context.PlayerGlobalScore
            .Include(pgs => pgs.PlayerEntity)
            .Include(pgs => pgs.PoolEntity)
            .Where(pgs => pgs.PoolEntity.Id == poolId)
            .OrderByDescending(pgs => pgs.TotalScore)
            .ThenBy(pgs => pgs.PlayerEntity.Username)
            .ToListAsync();

        var playerRankings = new List<PlayerRanking>();

        foreach (var globalScore in globalScores)
        {
            // Récupérer les scores d'activité pour ce joueur
            var activityScores = await _context.PlayerActivityScore
                .Include(pas => pas.ActivityEntity)
                .Include(pas => pas.PlayerEntity)
                .Include(pas => pas.PoolEntity)
                .Where(pas => pas.PlayerEntity.Id == globalScore.PlayerEntity.Id && pas.PoolEntity.Id == poolId)
                .Select(pas => new ActivityScore
                {
                    ActivityId = pas.ActivityEntity.Id,
                    ActivityName = pas.ActivityEntity.Name,
                    Score = pas.TotalScore,
                    Percentage = pas.Percentage,
                    Rank = pas.Rank
                })
                .ToListAsync();

            playerRankings.Add(new PlayerRanking
            {
                PlayerId = globalScore.PlayerEntity.Id,
                PlayerName = $"{globalScore.PlayerEntity.Firstname} {globalScore.PlayerEntity.Lastname}",
                Username = globalScore.PlayerEntity.Username,
                GlobalScore = globalScore.TotalScore,
                GlobalPercentage = globalScore.Percentage,
                GlobalRank = globalScore.GlobalRank,
                ActivitiesPlayed = globalScore.ActivitiesPlayed,
                ActivityScores = activityScores
            });
        }

        return playerRankings;
    }

    public async Task<List<PlayerRanking>> GetActivityRankingsAsync(Guid poolId, Guid activityId)
    {
        // Récupération directe depuis la BDD
        var activityScores = await _context.PlayerActivityScore
            .Include(pas => pas.PlayerEntity)
            .Include(pas => pas.PoolEntity)
            .Include(pas => pas.ActivityEntity)
            .Where(pas => pas.PoolEntity.Id == poolId && pas.ActivityEntity.Id == activityId)
            .OrderByDescending(pas => pas.TotalScore)
            .ThenBy(pas => pas.PlayerEntity.Username)
            .ToListAsync();

        var rankings = new List<PlayerRanking>();

        foreach (var activityScore in activityScores)
        {
            rankings.Add(new PlayerRanking
            {
                PlayerId = activityScore.PlayerEntity.Id,
                PlayerName = $"{activityScore.PlayerEntity.Firstname} {activityScore.PlayerEntity.Lastname}",
                Username = activityScore.PlayerEntity.Username,
                GlobalScore = activityScore.TotalScore,
                GlobalPercentage = activityScore.Percentage,
                GlobalRank = activityScore.Rank
            });
        }

        return rankings;
    }

    public async Task<PlayerRanking> GetPlayerRankingAsync(Guid poolId, Guid playerId)
    {
        var globalScore = await _context.PlayerGlobalScore
            .Include(pgs => pgs.PlayerEntity)
            .Include(pgs => pgs.PoolEntity)
            .FirstOrDefaultAsync(pgs => pgs.PoolEntity.Id == poolId && pgs.PlayerEntity.Id == playerId);

        if (globalScore == null) 
            throw new ArgumentException("Player not found in pool");

        // Récupérer les scores d'activité pour ce joueur
        var activityScores = await _context.PlayerActivityScore
            .Include(pas => pas.ActivityEntity)
            .Include(pas => pas.PlayerEntity)
            .Include(pas => pas.PoolEntity)
            .Where(pas => pas.PlayerEntity.Id == playerId && pas.PoolEntity.Id == poolId)
            .Select(pas => new ActivityScore
            {
                ActivityId = pas.ActivityEntity.Id,
                ActivityName = pas.ActivityEntity.Name,
                Score = pas.TotalScore,
                Percentage = pas.Percentage,
                Rank = pas.Rank
            })
            .ToListAsync();

        return new PlayerRanking
        {
            PlayerId = globalScore.PlayerEntity.Id,
            PlayerName = $"{globalScore.PlayerEntity.Firstname} {globalScore.PlayerEntity.Lastname}",
            Username = globalScore.PlayerEntity.Username,
            GlobalScore = globalScore.TotalScore,
            GlobalPercentage = globalScore.Percentage,
            GlobalRank = globalScore.GlobalRank,
            ActivitiesPlayed = globalScore.ActivitiesPlayed,
            ActivityScores = activityScores
        };
    }

    public async Task<double> CalculatePlayerGlobalScoreAsync(Guid poolId, Guid playerId)
    {
        var globalScore = await _context.PlayerGlobalScore
            .Include(pgs => pgs.PoolEntity)
            .Include(pgs => pgs.PlayerEntity)
            .FirstOrDefaultAsync(pgs => pgs.PoolEntity.Id == poolId && pgs.PlayerEntity.Id == playerId);

        return globalScore?.TotalScore ?? 0;
    }

    private async Task<List<ActivitySummary>> GetActivitySummariesAsync(Guid poolId)
    {
        // Récupération directe depuis la BDD
        var summaries = await _context.ActivitySummaryData
            .Include(ats => ats.ActivityEntity)
            .Include(ats => ats.PoolEntity)
            .Where(ats => ats.PoolEntity.Id == poolId)
            .Select(ats => new ActivitySummary
            {
                ActivityId = ats.ActivityEntity.Id,
                ActivityName = ats.ActivityEntity.Name,
                PlayersPlayed = ats.PlayersPlayed,
                BestScore = ats.BestScore,
                AverageScore = ats.AverageScore
            })
            .ToListAsync();

        return summaries;
    }
}
