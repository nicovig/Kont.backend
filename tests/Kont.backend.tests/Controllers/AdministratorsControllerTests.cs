using Kont.backend.Controllers;
using Kont.backend.DAL;
using Kont.backend.Services;
using Kont.backend.Models.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Kont.backend.tests.Controllers;

public class AdministratorsControllerTests
{
    private IAdministratorsService _service = null!;
    private ILogger<AdministratorsController> _logger = null!;
    private IUserContextService _userContextService = null!;
    private AdministratorsController _controller = null!;

    [SetUp]
    public void Setup()
    {
        _service = Substitute.For<IAdministratorsService>();
        _logger = Substitute.For<ILogger<AdministratorsController>>();
        _controller = new AdministratorsController(_service, _userContextService, _logger);
    }

    [Test]
    public async Task GetAll_ReturnsOk_WithList()
    {
        _service.GetAdministratorsAsync().Returns(new List<Administrator> { new Administrator { Firstname = "A", Lastname = "B", Email = "a@b.com", Password = "p", PhoneNumber = "0102030405", Subscription = new Subscription(), Role = new Role(), IsActive = true } });

        var result = await _controller.GetAll();

        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult!.StatusCode, Is.EqualTo(200));
    }

    [Test]
    public async Task GetById_NotFound()
    {
        _service.GetAdministratorAsync(Arg.Any<Guid>()).Returns((Administrator?)null);
        var result = await _controller.GetById(Guid.NewGuid());
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task Create_ReturnsCreated()
    {
        var createRequest = new CreateAdministratorRequest 
        { 
            Firstname = "A", 
            Lastname = "B", 
            Email = "a@b.com", 
            Password = "p", 
            PhoneNumber = "0102030405", 
            Role = new Role(), 
            IsActive = true,
            Sites = new List<Site>(),
            SubscriptionType = SubscriptionType.Stroll
        };
        var admin = new Administrator { Id = Guid.NewGuid(), Firstname = "A", Lastname = "B", Email = "a@b.com", Password = "p", PhoneNumber = "0102030405", Subscription = new Subscription(), Role = new Role(), IsActive = true };
        _service.CreateAdministratorAsync(Arg.Any<CreateAdministratorRequest>()).Returns(admin);
        var result = await _controller.Create(createRequest);
        Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
    }

    [Test]
    public async Task Roles_ReturnsOk()
    {
        _service.GetRolesAsync().Returns(new List<Role> { new Role { Id = Guid.NewGuid(), RoleType = RoleType.Admin } });
        var result = await _controller.GetRoles();
        var objectResult = result as ObjectResult;
        Assert.That(objectResult, Is.Not.Null);
        Assert.That(objectResult!.StatusCode, Is.EqualTo(200));
        var roles = objectResult.Value as IEnumerable<Role>;
        Assert.That(roles, Is.Not.Null);
        Assert.That(roles!.Any(r => r.RoleType == RoleType.Admin));
    }

    [Test]
    public async Task Update_NotFound_WhenMissing()
    {
        _service.UpdateAdministratorAsync(Arg.Any<Guid>(), Arg.Any<Administrator>()).Returns((Administrator?)null);
        var result = await _controller.Update(Guid.NewGuid(), new Administrator { IsActive = true });
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task Delete_NoContent_WhenExists()
    {
        _service.DeleteAdministratorAsync(Arg.Any<Guid>()).Returns(true);
        var result = await _controller.Delete(Guid.NewGuid());
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }
}


