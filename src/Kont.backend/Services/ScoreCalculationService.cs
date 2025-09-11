using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace Kont.backend.Services;

public interface IScoreCalculationService
{
    Task RecalculatePoolScoresAsync(Guid poolId);
    Task RecalculatePlayerActivityScoreAsync(Guid poolId, Guid playerId, Guid activityId);
    Task RecalculatePlayerGlobalScoreAsync(Guid poolId, Guid playerId);
}

public class ScoreCalculationService : IScoreCalculationService
{
    private readonly IDatabaseContext _context;

    public ScoreCalculationService(IDatabaseContext context)
    {
        _context = context;
    }

    public async Task RecalculatePoolScoresAsync(Guid poolId)
    {
        var pool = await _context.Pool.FindAsync(poolId);
        if (pool == null) throw new ArgumentException("Pool not found");

        // 1. Recalculer tous les scores d'activité
        await RecalculateAllActivityScoresAsync(poolId);

        // 2. Recalculer tous les scores globaux
        await RecalculateAllGlobalScoresAsync(poolId);

        // 3. Mettre à jour les résumés d'activités
        await UpdateActivitySummariesAsync(poolId);

        await _context.SaveChangesAsync();
    }

    public async Task RecalculatePlayerActivityScoreAsync(Guid poolId, Guid playerId, Guid activityId)
    {
        var playerScores = _context.PlayerScore
            .Where(ps => ps.Player.Id == playerId && ps.Activity.Id == activityId)
            .ToList();

        if (!playerScores.Any()) return;

        double totalScore = 0;
        foreach (var score in playerScores)
        {
            var normalizedValue = NormalizeScore(score.Value, score.ScoringMetric);
            totalScore += normalizedValue * score.ScoringMetric.Coefficient;
        }

        // Récupérer ou créer le score d'activité
        var activityScore = await _context.PlayerActivityScore
            .Include(pas => pas.PlayerEntity)
            .Include(pas => pas.ActivityEntity)
            .Include(pas => pas.PoolEntity)
            .FirstOrDefaultAsync(pas => pas.PlayerEntity.Id == playerId && 
                                       pas.ActivityEntity.Id == activityId && 
                                       pas.PoolEntity.Id == poolId);

        if (activityScore == null)
        {
            // Récupérer les entités référencées
            var player = await _context.Player.FindAsync(playerId);
            var activity = await _context.Activity.FindAsync(activityId);
            var pool = await _context.Pool.FindAsync(poolId);

            if (player == null || activity == null || pool == null)
                throw new ArgumentException("Player, Activity, or Pool not found");

            activityScore = new PlayerActivityScore
            {
                PlayerEntity = player,
                ActivityEntity = activity,
                PoolEntity = pool,
                TotalScore = totalScore
            };
            _context.PlayerActivityScore.Add(activityScore);
        }
        else
        {
            activityScore.TotalScore = totalScore;
            activityScore.LastUpdatedAt = DateTime.UtcNow;
        }

        // Calculer le pourcentage et le rang
        await UpdateActivityScoreRankingAsync(poolId, activityId);

        await _context.SaveChangesAsync();
    }

    public async Task RecalculatePlayerGlobalScoreAsync(Guid poolId, Guid playerId)
    {
        var activityScores = await _context.PlayerActivityScore
            .Include(pas => pas.PlayerEntity)
            .Include(pas => pas.PoolEntity)
            .Where(pas => pas.PlayerEntity.Id == playerId && pas.PoolEntity.Id == poolId)
            .ToListAsync();

        double totalScore = 0;
        foreach (var acs in activityScores)
        {
            totalScore += acs.TotalScore;
        }
        var activitiesPlayed = activityScores.Count;

        // Récupérer ou créer le score global
        var globalScore = await _context.PlayerGlobalScore
            .Include(pgs => pgs.PlayerEntity)
            .Include(pgs => pgs.PoolEntity)
            .FirstOrDefaultAsync(pgs => pgs.PlayerEntity.Id == playerId && pgs.PoolEntity.Id == poolId);

        if (globalScore == null)
        {
            // Récupérer les entités référencées
            var player = await _context.Player.FindAsync(playerId);
            var pool = await _context.Pool.FindAsync(poolId);

            if (player == null || pool == null)
                throw new ArgumentException("Player or Pool not found");

            globalScore = new PlayerGlobalScore
            {
                PlayerEntity = player,
                PoolEntity = pool,
                TotalScore = totalScore,
                ActivitiesPlayed = activitiesPlayed
            };
            _context.PlayerGlobalScore.Add(globalScore);
        }
        else
        {
            globalScore.TotalScore = totalScore;
            globalScore.ActivitiesPlayed = activitiesPlayed;
            globalScore.LastUpdatedAt = DateTime.UtcNow;
        }

        // Calculer le pourcentage et le rang global
        await UpdateGlobalScoreRankingAsync(poolId);

        await _context.SaveChangesAsync();
    }

    private async Task RecalculateAllActivityScoresAsync(Guid poolId)
    {
        var pool = await _context.Pool.FindAsync(poolId);
        if (pool == null) return;

        var completedSessions = pool.GameSessions.Where(gs => gs.Status == GameSessionStatus.Completed);

        foreach (var session in completedSessions)
        {
            foreach (var registration in pool.PlayerRegistrations)
            {
                await RecalculatePlayerActivityScoreAsync(poolId, registration.Id, session.Activity.Id);
            }
        }
    }

    private async Task RecalculateAllGlobalScoresAsync(Guid poolId)
    {
        var pool = await _context.Pool.FindAsync(poolId);
        if (pool == null) return;

        foreach (var registration in pool.PlayerRegistrations)
        {
            await RecalculatePlayerGlobalScoreAsync(poolId, registration.Id);
        }
    }

    private async Task UpdateActivityScoreRankingAsync(Guid poolId, Guid activityId)
    {
        var activityScores = await _context.PlayerActivityScore
            .Include(pas => pas.PoolEntity)
            .Include(pas => pas.ActivityEntity)
            .Where(pas => pas.PoolEntity.Id == poolId && pas.ActivityEntity.Id == activityId)
            .OrderByDescending(pas => pas.TotalScore)
            .ToListAsync();

        var bestScore = activityScores.FirstOrDefault()?.TotalScore ?? 0;

        for (int i = 0; i < activityScores.Count; i++)
        {
            var score = activityScores[i];
            score.Rank = i + 1;
            score.Percentage = bestScore > 0 ? Math.Round((score.TotalScore / bestScore) * 100, 1) : 0;
            score.LastUpdatedAt = DateTime.UtcNow;
        }
    }

    private async Task UpdateGlobalScoreRankingAsync(Guid poolId)
    {
        var globalScores = await _context.PlayerGlobalScore
            .Include(pgs => pgs.PoolEntity)
            .Where(pgs => pgs.PoolEntity.Id == poolId)
            .OrderByDescending(pgs => pgs.TotalScore)
            .ToListAsync();

        var bestScore = globalScores.FirstOrDefault()?.TotalScore ?? 0;

        for (int i = 0; i < globalScores.Count; i++)
        {
            var score = globalScores[i];
            score.GlobalRank = i + 1;
            score.Percentage = bestScore > 0 ? Math.Round((score.TotalScore / bestScore) * 100, 1) : 0;
            score.LastUpdatedAt = DateTime.UtcNow;
        }
    }

    private async Task UpdateActivitySummariesAsync(Guid poolId)
    {
        var pool = await _context.Pool.FindAsync(poolId);
        if (pool == null) return;

        var completedSessions = pool.GameSessions.Where(gs => gs.Status == GameSessionStatus.Completed);

        foreach (var session in completedSessions)
        {
            var activityScores = await _context.PlayerActivityScore
                .Include(pas => pas.PoolEntity)
                .Include(pas => pas.ActivityEntity)
                .Where(pas => pas.PoolEntity.Id == poolId && pas.ActivityEntity.Id == session.Activity.Id)
                .ToListAsync();

            if (activityScores.Any())
            {
                var summary = await _context.ActivitySummaryData
                    .Include(ats => ats.PoolEntity)
                    .Include(ats => ats.ActivityEntity)
                    .FirstOrDefaultAsync(ats => ats.PoolEntity.Id == poolId && ats.ActivityEntity.Id == session.Activity.Id);

                if (summary == null)
                {
                    // Récupérer les entités référencées
                    var activity = await _context.Activity.FindAsync(session.Activity.Id);
                    var poolEntity = await _context.Pool.FindAsync(poolId);

                    if (activity == null || poolEntity == null)
                        throw new ArgumentException("Activity or Pool not found");

                    summary = new ActivitySummaryData
                    {
                        ActivityEntity = activity,
                        PoolEntity = poolEntity
                    };
                    _context.ActivitySummaryData.Add(summary);
                }

                summary.PlayersPlayed = activityScores.Count;
                double bestScore = 0;
                double totalScore = 0;
                foreach (var acs in activityScores)
                {
                    if (acs.TotalScore > bestScore) bestScore = acs.TotalScore;
                    totalScore += acs.TotalScore;
                }
                summary.BestScore = bestScore;
                summary.AverageScore = Math.Round(totalScore / activityScores.Count, 2);
                summary.LastUpdatedAt = DateTime.UtcNow;
            }
        }
    }

    private double NormalizeScore(double value, ScoringMetric metric)
    {
        if (metric.HigherIsBetter)
        {
            return value;
        }
        else
        {
            return 1000 - value;
        }
    }
}
