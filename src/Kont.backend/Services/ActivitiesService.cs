using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Kont.backend.Models.Request;

namespace Kont.backend.Services;

public interface IActivitiesService
{
    Task<IEnumerable<Activity>> GetActivitiesAsync();
    Task<Activity?> GetActivityByIdAsync(Guid id);
    Task<Activity> CreateActivityAsync(Activity activity);
    Task<Activity?> UpdateActivityAsync(Guid id, UpdateActivityRequest updateActivityRequest);
    Task<bool> DeleteActivityAsync(Guid id);
    Task<ActivitySummaryData?> GetActivitySummaryAsync(Guid poolId, Guid activityId);
}

public class ActivitiesService : IActivitiesService
{
    private readonly IDatabaseContext _context;

    public ActivitiesService(IDatabaseContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Activity>> GetActivitiesAsync()
    {
        return await _context.Activity
            .Include(a => a.Site)
            .Include(a => a.ScoringMetrics)
            .Include(a => a.CreatedBy)
            .ToListAsync();
    }

    public async Task<Activity?> GetActivityByIdAsync(Guid id)
    {
        return await _context.Activity
            .Include(a => a.Site)
            .Include(a => a.ScoringMetrics)
            .Include(a => a.CreatedBy)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Activity> CreateActivityAsync(Activity activity)
    {
        var sum = activity.ScoringMetrics.Sum(m => m.Coefficient);
        if (Math.Abs(sum - 1) > 1e-6)
        {
            throw new ArgumentException("Sum of coefficients must equal 1");
        }

        if (activity.Site != null)
        {
            var siteId = activity.Site.Id;
            if (siteId != Guid.Empty)
            {
                var existingSite = await _context.Site.FindAsync(siteId);
                if (existingSite == null)
                {
                    throw new ArgumentException("Site not found");
                }
                activity.Site = existingSite;
            }
        }

        if (activity.CreatedBy != null)
        {
            var adminId = activity.CreatedBy.Id;
            if (adminId != Guid.Empty)
            {
                var admin = await _context.Administrator.FindAsync(adminId);
                if (admin == null)
                {
                    throw new ArgumentException("Administrator not found");
                }
                activity.CreatedBy = admin;
            }
        }

        activity.Id = Guid.NewGuid();
        activity.CreatedAt = DateTime.UtcNow;

        _context.Activity.Add(activity);
        await _context.SaveChangesAsync();

        return activity;
    }

    public async Task<Activity?> UpdateActivityAsync(Guid id, UpdateActivityRequest updateActivityRequest)
    {
        var existingActivity = await _context.Activity.FindAsync(id);
        if (existingActivity == null)
        {
            return null;
        }

        var sum = updateActivityRequest.ScoringMetrics.Sum(m => m.Coefficient);
        if (Math.Abs(sum - 1) > 1e-6)
        {
            throw new ArgumentException("Sum of coefficients must equal 1");
        }

        existingActivity.Name = updateActivityRequest.Name;
        existingActivity.Description = updateActivityRequest.Description;
        if (updateActivityRequest.Site != null)
        {
            var siteId = updateActivityRequest.Site.Id;
            if (siteId != Guid.Empty)
            {
                var site = await _context.Site.FindAsync(siteId);
                if (site == null)
                {
                    throw new ArgumentException("Site not found");
                }
                existingActivity.Site = site;
            }
        }
        existingActivity.ScoringMetrics = updateActivityRequest.ScoringMetrics
            .Select(sm => new ScoringMetric
            {
                Name = sm.Name,
                Unit = sm.Unit,
                HigherIsBetter = sm.HigherIsBetter,
                Coefficient = sm.Coefficient
            })
            .ToList();

        await _context.SaveChangesAsync();

        return existingActivity;
    }

    public async Task<bool> DeleteActivityAsync(Guid id)
    {
        var activity = await _context.Activity.FindAsync(id);
        if (activity == null)
        {
            return false;
        }

        _context.Activity.Remove(activity);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<ActivitySummaryData?> GetActivitySummaryAsync(Guid poolId, Guid activityId)
    {
        return await _context.ActivitySummaryData
            .Include(a => a.ActivityEntity)
            .Include(a => a.PoolEntity)
            .FirstOrDefaultAsync(a => a.PoolEntity.Id == poolId && a.ActivityEntity.Id == activityId);
    }
}
