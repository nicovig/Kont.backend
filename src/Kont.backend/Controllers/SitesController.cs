using Microsoft.AspNetCore.Mvc;
using Kont.backend.Services;
using Kont.backend.DAL;

namespace Kont.backend.Controllers;

[Route("[controller]")]
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
        var sites = await _sitesService.GetSitesAsync();
        return Ok(sites);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSite([FromRoute] Guid id)
    {
        var site = await _sitesService.GetSiteAsync(id);
        if (site == null)
        {
            return NotFound(new { message = "Site not found" });
        }
        return Ok(site);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSite([FromBody] Site site)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var created = await _sitesService.CreateSiteAsync(site);
        return CreatedAtAction(nameof(GetSites), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSite([FromRoute] Guid id, [FromBody] Site site)
    {
        var updated = await _sitesService.UpdateSiteAsync(id, site);
        if (updated == null)
        {
            return NotFound(new { message = "Site not found" });
        }
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSite([FromRoute] Guid id)
    {
        var ok = await _sitesService.DeleteSiteAsync(id);
        if (!ok)
        {
            return NotFound(new { message = "Site not found" });
        }
        return NoContent();
    }
}


