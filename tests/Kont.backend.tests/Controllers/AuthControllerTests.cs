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
        var jwtResponse = new JwtResponse
        {
            Token = "jwt_token_here",
            UserId = Guid.NewGuid(),
            Role = "Admin",
            Email = request.Email,
            Firstname = "Admin",
            Lastname = "User",
            SubscriptionType = SubscriptionType.Klasel,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        _authService.LoginAsync(request.Email, request.Password, RoleType.Admin).Returns(jwtResponse);

        var result = await _controller.Login(request);

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(200));
        Assert.That(objectResult.Value, Is.InstanceOf<JwtResponse>());
        var returned = (JwtResponse)objectResult.Value!;
        Assert.That(returned.Email, Is.EqualTo(request.Email));
        Assert.That(returned.Role, Is.EqualTo("Admin"));
        Assert.That(returned.Token, Is.Not.Null);
        Assert.That(returned.UserId, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var request = new LoginRequest { Email = "admin@test.com", Password = "wrong" };

        _authService.LoginAsync(request.Email, request.Password, RoleType.Admin)
            .Returns(Task.FromException<JwtResponse>(new ArgumentException("Invalid credentials")));

        var result = await _controller.Login(request);

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(401));
    }
}


