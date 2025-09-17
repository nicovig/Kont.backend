using Microsoft.AspNetCore.Mvc;
using Kont.backend.Services;
using Kont.backend.DAL;
using Kont.backend.Models.Request;
using Microsoft.AspNetCore.Authorization;

namespace Kont.backend.Controllers;

[Route("[controller]")]
[Authorize(Roles = nameof(RoleType.God))]
[ApiController]
public class AdministratorsController : ControllerBase
{
    private readonly IAdministratorsService _service;
    private readonly ILogger<AdministratorsController> _logger;

    public AdministratorsController(IAdministratorsService service, ILogger<AdministratorsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _service.GetRolesAsync();
        return Ok(roles);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _service.GetAdministratorsAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var admin = await _service.GetAdministratorAsync(id);
        if (admin == null) return NotFound(new { message = "Administrator not found" });
        return Ok(admin);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAdministratorRequest createRequest)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var created = await _service.CreateAdministratorAsync(createRequest);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Administrator admin)
    {
        var updated = await _service.UpdateAdministratorAsync(id, admin);
        if (updated == null) return NotFound(new { message = "Administrator not found" });
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _service.DeleteAdministratorAsync(id);
        if (!ok) return NotFound(new { message = "Administrator not found" });
        return NoContent();
    }
}


