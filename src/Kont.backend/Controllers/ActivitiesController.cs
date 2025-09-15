using Microsoft.AspNetCore.Mvc;
using Kont.backend.DAL.DatabaseContext;
using Kont.backend.DAL;
using Microsoft.EntityFrameworkCore;
using Kont.backend.Models.Scoring;

namespace Kont.backend.Controllers;

[Route("api/admin/[controller]")]
[ApiController]
public class ActivitiesController : ControllerBase
{
    private readonly IDatabaseContext _context;
    private readonly ILogger<ActivitiesController> _logger;

    public ActivitiesController(
        IDatabaseContext context,
        ILogger<ActivitiesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all activities for the current admin
    /// </summary>
    /// <returns>List of activities</returns>
    /// <response code="200">Activities retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Activity>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetActivities()
    {
        try
        {
            var activities = await _context.Activity
                .Include(a => a.Site)
                .Include(a => a.ScoringMetrics)
                .Include(a => a.CreatedBy)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} activities", activities.Count);
            return Ok(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving activities");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get a specific activity by ID
    /// </summary>
    /// <param name="id">Activity ID</param>
    /// <returns>Activity details</returns>
    /// <response code="200">Activity found</response>
    /// <response code="404">Activity not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Activity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetActivity(Guid id)
    {
        try
        {
            var activity = await _context.Activity
                .Include(a => a.Site)
                .Include(a => a.ScoringMetrics)
                .Include(a => a.CreatedBy)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (activity == null)
            {
                _logger.LogWarning("Activity with ID {Id} not found", id);
                return NotFound(new { message = "Activity not found" });
            }

            return Ok(activity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving activity {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Create a new activity
    /// </summary>
    /// <param name="activity">Activity data</param>
    /// <returns>Created activity</returns>
    /// <response code="201">Activity created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost]
    [ProducesResponseType(typeof(Activity), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateActivity([FromBody] Activity activity)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            activity.Id = Guid.NewGuid();
            activity.CreatedAt = DateTime.UtcNow;

            _context.Activity.Add(activity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created activity {Id} with name {Name}", activity.Id, activity.Name);

            return CreatedAtAction(nameof(GetActivity), new { id = activity.Id }, activity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating activity");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Update an existing activity
    /// </summary>
    /// <param name="id">Activity ID</param>
    /// <param name="activity">Updated activity data</param>
    /// <returns>Updated activity</returns>
    /// <response code="200">Activity updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Activity not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Activity), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateActivity(Guid id, [FromBody] Activity activity)
    {
        try
        {
            if (id != activity.Id)
            {
                return BadRequest(new { message = "ID mismatch" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingActivity = await _context.Activity.FindAsync(id);
            if (existingActivity == null)
            {
                _logger.LogWarning("Activity with ID {Id} not found for update", id);
                return NotFound(new { message = "Activity not found" });
            }

            existingActivity.Name = activity.Name;
            existingActivity.Description = activity.Description;
            existingActivity.Site = activity.Site;
            existingActivity.ScoringMetrics = activity.ScoringMetrics;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated activity {Id}", id);
            return Ok(existingActivity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating activity {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete an activity
    /// </summary>
    /// <param name="id">Activity ID</param>
    /// <returns>No content</returns>
    /// <response code="204">Activity deleted successfully</response>
    /// <response code="404">Activity not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteActivity(Guid id)
    {
        try
        {
            var activity = await _context.Activity.FindAsync(id);
            if (activity == null)
            {
                _logger.LogWarning("Activity with ID {Id} not found for deletion", id);
                return NotFound(new { message = "Activity not found" });
            }

            _context.Activity.Remove(activity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted activity {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting activity {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get activity summary data for a specific pool
    /// </summary>
    /// <param name="poolId">Pool ID</param>
    /// <param name="activityId">Activity ID</param>
    /// <returns>Activity summary data</returns>
    /// <response code="200">Summary data retrieved successfully</response>
    /// <response code="404">Activity or pool not found</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet("pools/{poolId}/activities/{activityId}/summary")]
    [ProducesResponseType(typeof(ActivitySummaryData), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetActivitySummary(Guid poolId, Guid activityId)
    {
        try
        {
            var summary = await _context.ActivitySummaryData
                .Include(a => a.ActivityEntity)
                .Include(a => a.PoolEntity)
                .FirstOrDefaultAsync(a => a.PoolEntity.Id == poolId && a.ActivityEntity.Id == activityId);

            if (summary == null)
            {
                _logger.LogWarning("Activity summary not found for pool {PoolId} and activity {ActivityId}", poolId, activityId);
                return NotFound(new { message = "Activity summary not found" });
            }

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving activity summary for pool {PoolId} and activity {ActivityId}", poolId, activityId);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
