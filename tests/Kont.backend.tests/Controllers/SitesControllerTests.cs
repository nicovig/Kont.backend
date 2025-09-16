using Kont.backend.Controllers;
using Kont.backend.DAL;
using Kont.backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Kont.backend.tests.Controllers;

public class SitesControllerTests
{
    private ISitesService _sitesService = null!;
    private ILogger<SitesController> _logger = null!;
    private SitesController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _sitesService = Substitute.For<ISitesService>();
        _logger = Substitute.For<ILogger<SitesController>>();
        _controller = new SitesController(_sitesService, _logger);
    }

    [Test]
    public async Task GetSites_ReturnsOk_WithList()
    {
        var sites = new List<Site> { new Site { Id = Guid.NewGuid(), Name = "A", Address = "Adr", City = "City", ZipCode = "75001", Country = "FR", State = "IDF", PhoneNumber = "0102030405", Email = "a@a.com" } };
        _sitesService.GetSitesAsync().Returns(sites);

        var result = await _controller.GetSites();

        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult!.StatusCode, Is.EqualTo(200));
        Assert.That(objectResult.Value, Is.InstanceOf<IEnumerable<Site>>());
    }

    [Test]
    public async Task CreateSite_ReturnsCreated()
    {
        var site = new Site { Id = Guid.NewGuid(), Name = "A", Address = "Adr", City = "City", ZipCode = "75001", Country = "FR", State = "IDF", PhoneNumber = "0102030405", Email = "a@a.com" };
        _sitesService.CreateSiteAsync(Arg.Any<Site>()).Returns(site);

        var result = await _controller.CreateSite(site);

        Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
    }

    [Test]
    public async Task UpdateSite_NotFound_Returns404()
    {
        _sitesService.UpdateSiteAsync(Arg.Any<Guid>(), Arg.Any<Site>()).Returns((Site?)null);

        var result = await _controller.UpdateSite(Guid.NewGuid(), new Site());

        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task DeleteSite_NoContent_OnSuccess()
    {
        _sitesService.DeleteSiteAsync(Arg.Any<Guid>()).Returns(true);

        var result = await _controller.DeleteSite(Guid.NewGuid());

        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }
}


