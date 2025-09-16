using Microsoft.AspNetCore.Mvc;
using Kont.backend.DAL;
using Kont.backend.Services;

namespace Kont.backend.Controllers;

[Route("admin/[controller]")]
[ApiController]
public class ActivitiesController : ControllerBase
{
    private readonly IActivitiesService _activitiesService;
    private readonly ILogger<ActivitiesController> _logger;

    public ActivitiesController(
        IActivitiesService activitiesService,
        ILogger<ActivitiesController> logger)
    {
        _activitiesService = activitiesService;
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
            var activities = await _activitiesService.GetActivitiesAsync();

            _logger.LogInformation("Retrieved {Count} activities", activities.Count());
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
            var activity = await _activitiesService.GetActivityByIdAsync(id);

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

            var createdActivity = await _activitiesService.CreateActivityAsync(activity);

            _logger.LogInformation("Created activity {Id} with name {Name}", createdActivity.Id, createdActivity.Name);

            return CreatedAtAction(nameof(GetActivity), new { id = createdActivity.Id }, createdActivity);
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

            var updatedActivity = await _activitiesService.UpdateActivityAsync(id, activity);
            if (updatedActivity == null)
            {
                _logger.LogWarning("Activity with ID {Id} not found for update", id);
                return NotFound(new { message = "Activity not found" });
            }

            _logger.LogInformation("Updated activity {Id}", id);
            return Ok(updatedActivity);
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
            var deleted = await _activitiesService.DeleteActivityAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("Activity with ID {Id} not found for deletion", id);
                return NotFound(new { message = "Activity not found" });
            }

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
            var summary = await _activitiesService.GetActivitySummaryAsync(poolId, activityId);

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
