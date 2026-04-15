using IdentityModel.OidcClient;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Products.Application.Features.Authentication.Commands;
using System.Security.Claims;

namespace Products.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Base.BaseController
    {
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginCommand command)
        {
            var result = await Mediator.Send(command);

            var claims = new List<Claim>
                        {
                             new Claim(ClaimTypes.NameIdentifier, result.UserId.ToString()),
                             new Claim(ClaimTypes.Name, result.Username)
                         };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await JwtService.SetCookie("accessToken", result.Token, TimeSpan.FromMinutes(15));

            return Ok(true);
        }

        [HttpGet("me")]
        [Authorize]
        [Produces("application/json")]
        public IActionResult Me()
        {
            return new JsonResult(new { isAuthenticated = true });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await JwtService.DeleteCookie("accessToken");

            return Ok(new { message = "Logged out successfully" });
        }
    }
}
