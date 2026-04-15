using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Products.Api.Controllers;
using Products.Api.Tests.TestUtils;
using Products.Application.Common.Interfaces;
using Products.Application.Features.Authentication.Commands;
using Products.Application.Features.Authentication.Dtos;

namespace Products.Api.Tests.Api.Controllers;

public class AuthControllerTests
{
    [Fact]   
    public async    Task Login_ShouldReturnOkTrueAndSetCookie_WhenMediatorReturnsLoginResult()
    {
        // Arrange
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        var jwt = new Mock<IJwtService>(MockBehavior.Strict);
        var sut = ControllerTestFactory.CreateController<AuthController>(mediator.Object, jwt.Object);

        var command = new LoginCommand { Username = "superadmin", Password = "Admin@123" };

        var loginResult = new LoginResult
        {
            UserId = Guid.NewGuid(),
            Username = "superadmin",
            Token = "token"
        };

        mediator.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResult);

        jwt.Setup(j => j.SetCookie("accessToken", loginResult.Token, It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await sut.Login(command);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(true, ok.Value);

        mediator.Verify(m => m.Send(command, It.IsAny<CancellationToken>()), Times.Once);
        jwt.Verify(j => j.SetCookie("accessToken", loginResult.Token, It.IsAny<TimeSpan>()), Times.Once);
        mediator.VerifyNoOtherCalls();
        jwt.VerifyNoOtherCalls();
    }

    [Fact]
    public void Logout_ShouldReturnOkWithMessage_WhenCalled()
    {
        // Arrange
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        var sut = ControllerTestFactory.CreateController<AuthController>(mediator.Object);

        // Act
        var result = sut.Logout();

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);

        var messageProp = ok.Value.GetType().GetProperty("message");
        Assert.NotNull(messageProp);
        Assert.Equal("Logged out successfully", messageProp!.GetValue(ok.Value));

        mediator.VerifyNoOtherCalls();
    }
}

