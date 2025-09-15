using Kont.backend.DAL;
using Kont.backend.DAL.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace Kont.backend.Services;

public interface IActivitiesService
{
    Task<IEnumerable<Activity>> GetActivitiesAsync();
    Task<Activity?> GetActivityByIdAsync(Guid id);
    Task<Activity> CreateActivityAsync(Activity activity);
    Task<Activity?> UpdateActivityAsync(Guid id, Activity activity);
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
        activity.Id = Guid.NewGuid();
        activity.CreatedAt = DateTime.UtcNow;

        _context.Activity.Add(activity);
        await _context.SaveChangesAsync();

        return activity;
    }

    public async Task<Activity?> UpdateActivityAsync(Guid id, Activity activity)
    {
        var existingActivity = await _context.Activity.FindAsync(id);
        if (existingActivity == null)
        {
            return null;
        }

        existingActivity.Name = activity.Name;
        existingActivity.Description = activity.Description;
        existingActivity.Site = activity.Site;
        existingActivity.ScoringMetrics = activity.ScoringMetrics;

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
