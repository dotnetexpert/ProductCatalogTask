using Moq;
using Products.Application.Common.Interfaces;
using Products.Application.Features.Authentication.Commands;
using Products.Application.Features.Authentication.Handlers;

namespace Products.Api.Tests.Application.Features.Authentication.Handlers;

public class LoginHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var jwt = new Mock<IJwtService>(MockBehavior.Strict);
        var sut = new LoginHandler(jwt.Object);

        var command = new LoginCommand
        {
            Username = "superadmin",
            Password = "Admin@123"
        };

        jwt.Setup(j => j.GenerateToken(It.IsAny<Guid>(), command.Username)).Returns("token");

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(command.Username, result.Username);
        Assert.Equal("token", result.Token);
        Assert.NotEqual(Guid.Empty, result.UserId);
        jwt.Verify(j => j.GenerateToken(It.IsAny<Guid>(), command.Username), Times.Once);
        jwt.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("superadmin", "wrong")]
    [InlineData("wrong", "Admin@123")]
    [InlineData("wrong", "wrong")]
    public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenCredentialsAreInvalid(string username, string password)
    {
        // Arrange
        var jwt = new Mock<IJwtService>(MockBehavior.Strict);
        var sut = new LoginHandler(jwt.Object);

        var command = new LoginCommand
        {
            Username = username,
            Password = password
        };

        // Act
        var act = () => sut.Handle(command, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(act);
        Assert.Equal("Invalid credentials", ex.Message);
        jwt.VerifyNoOtherCalls();
    }
}

