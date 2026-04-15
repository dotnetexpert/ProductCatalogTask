using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Products.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Products.Application.Common.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtService(IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            _config = config;
            _httpContextAccessor = httpContextAccessor;
        }

        public string GenerateToken(Guid userId, string username)
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username)
        };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public Task SetCookie(string key, string value, TimeSpan duration)
        {
            var context = _httpContextAccessor.HttpContext!;

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // use false for localhost; true for production
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.Add(duration),
                Path = "/"
            };

            context.Response.Cookies.Append(key, value, cookieOptions);
            return Task.CompletedTask;
        }

        public Task DeleteCookie(string key)
        {
            var context = _httpContextAccessor.HttpContext!;
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // production
                SameSite = SameSiteMode.None, // if cross-site
                Expires = DateTime.UtcNow.AddDays(-1),
                Path = "/"
            };

            context.Response.Cookies.Delete(key, cookieOptions);
            return Task.CompletedTask;
        }
    }
}
