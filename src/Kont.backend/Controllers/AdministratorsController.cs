using Microsoft.AspNetCore.Mvc;
using Kont.backend.Services;
using Kont.backend.DAL;
using Kont.backend.Models.Request;
using Microsoft.AspNetCore.Authorization;

namespace Kont.backend.Controllers;

[Route("[controller]")]
[ApiController]
public class AdministratorsController : ControllerBase
{
    private readonly IAdministratorsService _service;
    private readonly IUserContextService _userContextService;
    private readonly ILogger<AdministratorsController> _logger;

    public AdministratorsController(IAdministratorsService service, IUserContextService userContextService, ILogger<AdministratorsController> logger)
    {
        _service = service;
        _userContextService = userContextService;
        _logger = logger;
    }

    [HttpGet("current")]
    [Authorize(Roles = nameof(RoleType.Admin))]
    public async Task<IActionResult> GetCurrent()
    {
        var admin = await _userContextService.GetCurrentUserAsync();
        if (admin == null) return NotFound(new { message = "Administrator not found" });
        return Ok(admin);
    }

    [HttpGet("roles")]
    [Authorize(Roles = nameof(RoleType.God))]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _service.GetRolesAsync();
        return Ok(roles);
    }

    [HttpGet]
    [Authorize(Roles = nameof(RoleType.God))]
    public async Task<IActionResult> GetAll()
    {
        var list = await _service.GetAdministratorsAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = nameof(RoleType.God))]
    public async Task<IActionResult> GetById(Guid id)
    {
        var admin = await _service.GetAdministratorAsync(id);
        if (admin == null) return NotFound(new { message = "Administrator not found" });
        return Ok(admin);
    }

    [HttpPost]
    [Authorize(Roles = nameof(RoleType.God))]
    public async Task<IActionResult> Create([FromBody] CreateAdministratorRequest createRequest)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var created = await _service.CreateAdministratorAsync(createRequest);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = nameof(RoleType.God))]
    public async Task<IActionResult> Update(Guid id, [FromBody] Administrator admin)
    {
        var updated = await _service.UpdateAdministratorAsync(id, admin);
        if (updated == null) return NotFound(new { message = "Administrator not found" });
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(RoleType.God))]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _service.DeleteAdministratorAsync(id);
        if (!ok) return NotFound(new { message = "Administrator not found" });
        return NoContent();
    }
}


