using Microsoft.AspNetCore.Mvc;
using Kont.backend.Services;
using Kont.backend.DAL;

namespace Kont.backend.Controllers;

[Route("[controller]")]
[ApiController]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionsService _service;
    private readonly ILogger<SubscriptionsController> _logger;

    public SubscriptionsController(ISubscriptionsService service, ILogger<SubscriptionsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _service.GetSubscriptionsAsync();
        return Ok(list);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var sub = await _service.GetSubscriptionAsync(id);
        if (sub == null) return NotFound(new { message = "Subscription not found" });
        return Ok(sub);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Subscription sub)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var created = await _service.CreateSubscriptionAsync(sub);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Subscription sub)
    {
        var updated = await _service.UpdateSubscriptionAsync(id, sub);
        if (updated == null) return NotFound(new { message = "Subscription not found" });
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _service.DeleteSubscriptionAsync(id);
        if (!ok) return NotFound(new { message = "Subscription not found" });
        return NoContent();
    }
}


