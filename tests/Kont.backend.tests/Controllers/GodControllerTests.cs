using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Kont.backend.Controllers;
using Kont.backend.DAL;
using Kont.backend.Models.User;
using Kont.backend.Services;

namespace Kont.backend.tests.Controllers;

[TestFixture]
public class GodControllerTests
{
    private IAuthService _authService;
    private ILogger<GodController> _logger;
    private GodController _controller;

    [SetUp]
    public void Setup()
    {
        _authService = Substitute.For<IAuthService>();
        _logger = Substitute.For<ILogger<GodController>>();
        _controller = new GodController(_authService, _logger);
    }

    [Test]
    public async Task Login_WithValidGodCredentials_ReturnsOk()
    {
        var request = new LoginRequest { Email = "god@test.com", Password = "password" };
        var user = new Administrator
        {
            Id = Guid.NewGuid(),
            Firstname = "God",
            Lastname = "User",
            Email = request.Email,
            Password = "hashed",
            PhoneNumber = "+33123456789",
            Subscription = new Subscription { Id = Guid.NewGuid() },
            Sites = new List<Site>(),
            Role = new Role { Id = Guid.NewGuid(), RoleType = RoleType.God },
            IsActive = true
        };

        _authService.LoginAsync(request.Email, request.Password, RoleType.God).Returns(user);

        var result = await _controller.Login(request);

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(200));
        var returned = (Administrator)objectResult.Value!;
        Assert.That(returned.Role.RoleType, Is.EqualTo(RoleType.God));
    }

    [Test]
    public async Task Login_WithInvalidRole_ReturnsUnauthorized()
    {
        var request = new LoginRequest { Email = "god@test.com", Password = "password" };

        _authService.LoginAsync(request.Email, request.Password, RoleType.God)
            .Returns(Task.FromException<Administrator>(new ArgumentException("Invalid credentials")));

        var result = await _controller.Login(request);

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(401));
    }
}


