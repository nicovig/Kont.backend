using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Kont.backend.Controllers;
using Kont.backend.DAL;
using Kont.backend.Models.User;
using Kont.backend.Services;

namespace Kont.backend.tests.Controllers;

[TestFixture]
public class AuthControllerTests
{
    private IAuthService _authService;
    private ILogger<AuthController> _logger;
    private AuthController _controller;

    [SetUp]
    public void Setup()
    {
        _authService = Substitute.For<IAuthService>();
        _logger = Substitute.For<ILogger<AuthController>>();
        _controller = new AuthController(_authService, _logger);
    }

    [Test]
    public async Task Login_WithValidAdminCredentials_ReturnsOk()
    {
        var request = new LoginRequest { Email = "admin@test.com", Password = "password" };
        var admin = new Administrator
        {
            Id = Guid.NewGuid(),
            Firstname = "Admin",
            Lastname = "User",
            Email = request.Email,
            Password = "hashed",
            PhoneNumber = "+33123456789",
            Subscription = new Subscription { Id = Guid.NewGuid() },
            Sites = new List<Site>(),
            Role = new Role { Id = Guid.NewGuid(), RoleType = RoleType.Admin },
            IsActive = true
        };

        _authService.LoginAsync(request.Email, request.Password, RoleType.Admin).Returns(admin);

        var result = await _controller.Login(request);

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(200));
        Assert.That(objectResult.Value, Is.InstanceOf<Administrator>());
        var returned = (Administrator)objectResult.Value!;
        Assert.That(returned.Email, Is.EqualTo(request.Email));
        Assert.That(returned.Role.RoleType, Is.EqualTo(RoleType.Admin));
    }

    [Test]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var request = new LoginRequest { Email = "admin@test.com", Password = "wrong" };

        _authService.LoginAsync(request.Email, request.Password, RoleType.Admin)
            .Returns(Task.FromException<Administrator>(new ArgumentException("Invalid credentials")));

        var result = await _controller.Login(request);

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(401));
    }
}


