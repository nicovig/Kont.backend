using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Kont.backend.tests.Tools;

internal static class HelperForTests
{
    internal static void SetUser(this ControllerBase controller, TestUser user, Claim[]? extraClaims = null)
    {
        List<Claim> claims =
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, "test@galarne.fr"),
            new Claim(ClaimTypes.Email, user.Email)
        ];

        if (extraClaims != null)
        {
            claims.AddRange(extraClaims);
        }

        var aspUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthentication"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = aspUser }
        };
    }

}
