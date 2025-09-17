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
        var jwtResponse = new JwtResponse
        {
            Token = "jwt_token_here",
            UserId = Guid.NewGuid(),
            Role = "God",
            Email = request.Email,
            Firstname = "God",
            Lastname = "User",
            SubscriptionType = SubscriptionType.Klasel,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        _authService.LoginAsync(request.Email, request.Password, RoleType.God).Returns(jwtResponse);

        var result = await _controller.Login(request);

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(200));
        var returned = (JwtResponse)objectResult.Value!;
        Assert.That(returned.Role, Is.EqualTo("God"));
        Assert.That(returned.Token, Is.Not.Null);
        Assert.That(returned.UserId, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public async Task Login_WithInvalidRole_ReturnsUnauthorized()
    {
        var request = new LoginRequest { Email = "god@test.com", Password = "password" };

        _authService.LoginAsync(request.Email, request.Password, RoleType.God)
            .Returns(Task.FromException<JwtResponse>(new ArgumentException("Invalid credentials")));

        var result = await _controller.Login(request);

        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;
        Assert.That(objectResult.StatusCode, Is.EqualTo(401));
    }
}


