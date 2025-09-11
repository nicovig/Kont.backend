using Kont.backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kont.backend.Controllers;

[Route("[controller]")]
[ApiController]
[AllowAnonymous]
public class ApplicationController : Controller
{
    static readonly ApplicationVersion? version;

    static ApplicationController()
    {
        version = new ApplicationVersion
        {
            Back = Environment.GetEnvironmentVariable("APP_VERSION_BACK") ?? "dev",
            Front = Environment.GetEnvironmentVariable("APP_VERSION_FRONT") ?? "not-defined"
        };
    }

    [HttpGet("versions")]
    [ProducesResponseType(typeof(ApplicationVersion), StatusCodes.Status200OK)]
    public IActionResult Versions()
    {
        return Ok(version);
    }

#if DEBUG
    [Route("/")]
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
#endif
}
