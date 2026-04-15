using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Products.Application.Common.Interfaces;

namespace Products.Api.Tests.TestUtils;

internal static class ControllerTestFactory
{
    public static TController CreateController<TController>(
        IMediator mediator,
        IJwtService? jwtService = null)
        where TController : ControllerBase, new()
    {
        var services = new ServiceCollection();
        services.AddSingleton(mediator);

        if (jwtService != null)
        {
            services.AddSingleton(jwtService);
        }

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };

        var controller = new TController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };

        return controller;
    }
}

