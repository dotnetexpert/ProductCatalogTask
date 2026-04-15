using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Products.Api.Contracts.Common;
using Products.Application.Common.Interfaces;
using Products.Application.Common.Services;

namespace Products.Api.Controllers.Base
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        private IMediator? _mediator;
        private IJwtService? _jwtService;

        protected IMediator Mediator =>
            _mediator ??= HttpContext.RequestServices.GetService<IMediator>()!;

        protected IJwtService JwtService =>
          _jwtService ??= HttpContext.RequestServices.GetService<IJwtService>()!;

        protected IActionResult Success<T>(T data, string? message = null)
        {
            return Ok(ApiResponse<T>.SuccessResponse(data, message));
        }

        protected IActionResult CreatedResponse<T>(T data, string? message = null)
        {
            return Created("", ApiResponse<T>.SuccessResponse(data, message));
        }

        protected IActionResult Failure(string message, List<string>? errors = null)
        {
            return BadRequest(ApiResponse<object>.FailureResponse(message, errors));
        }
    }
}
