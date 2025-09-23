using Microsoft.AspNetCore.Mvc;
using Kont.backend.DAL;
using Kont.backend.Services;
using Microsoft.AspNetCore.Authorization;

namespace Kont.backend.Controllers;

[Route("[controller]")]
[Authorize(Roles = nameof(RoleType.God))]
[ApiController]
public class SitesController : ControllerBase
{
    private readonly ISitesService _sitesService;
    private readonly ILogger<SitesController> _logger;

    public SitesController(ISitesService sitesService, ILogger<SitesController> logger)
    {
        _sitesService = sitesService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetSites()
    {
        try
        {
            var sites = await _sitesService.GetSitesAsync();
            return Ok(sites);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sites");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSite([FromRoute] Guid id)
    {
        try
        {
            var site = await _sitesService.GetSiteAsync(id);
            if (site == null)
            {
                return NotFound(new { message = "Site not found" });
            }
            return Ok(site);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving site {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateSite([FromBody] Site site)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var created = await _sitesService.CreateSiteAsync(site);
            return CreatedAtAction(nameof(GetSite), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating site");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSite([FromRoute] Guid id, [FromBody] Site site)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var updated = await _sitesService.UpdateSiteAsync(id, site);
            if (updated == null)
            {
                return NotFound(new { message = "Site not found" });
            }
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating site {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSite([FromRoute] Guid id)
    {
        try
        {
            var ok = await _sitesService.DeleteSiteAsync(id);
            if (!ok)
            {
                return NotFound(new { message = "Site not found" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting site {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}


