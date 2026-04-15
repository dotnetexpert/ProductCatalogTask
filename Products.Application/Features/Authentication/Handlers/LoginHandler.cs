using MediatR;
using Products.Application.Common.Interfaces;
using Products.Application.Features.Authentication.Commands;
using Products.Application.Features.Authentication.Dtos;
using Products.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Products.Application.Features.Authentication.Handlers
{
    public class LoginHandler : IRequestHandler<LoginCommand, LoginResult>
    {
        private readonly IJwtService _jwtService;

        public LoginHandler(IJwtService jwtService)
        {
            _jwtService = jwtService;
        }

        public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // Static user validation
            if (request.Username.ToLower() != "superadmin" || request.Password != "Admin@123")
                throw new UnauthorizedAccessException("Invalid credentials");

            var userId = Guid.Parse("ddab4c03-6f26-4b19-acb0-7992a79fa408");

            var token = _jwtService.GenerateToken(userId, request.Username);

            return new LoginResult
            {
                UserId = userId,
                Username = request.Username,
                Token = token
            };
        }
    }
}
