using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Kont.backend.Tools;

public static class UserHelper
{
    public static Guid? TryGetId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);

        return value != null ? Guid.Parse(value) : null;
    }
    public static Guid GetIdWithLoggedUser(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.Parse(value!);
    }

    public static string? GetEmail(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.Email);

        return value;
    }

    public static string RetrieveToken(this ControllerBase controller)
    {
        return RetrieveToken(controller.Request);
    }

    public static string RetrieveToken(this HttpRequest request)
    {
        string token;
        if (request.Headers.Authorization.Count > 0)
        {
            token = request.Headers.Authorization[0]!.Replace("Bearer ", "");
        }
        else
        {
            token = request.Query["access_token"]!;
        }
        return token;
    }
}