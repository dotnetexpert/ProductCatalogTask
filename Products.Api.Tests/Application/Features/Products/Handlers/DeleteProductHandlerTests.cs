using Moq;
using Products.Application.Features.Products.Commands;
using Products.Application.Features.Products.Handlers;
using Products.Domain.Entities;
using Products.Domain.Interfaces;

namespace Products.Api.Tests.Application.Features.Products.Handlers;

public class DeleteProductHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnProductId_WhenProductExists()
    {
        // Arrange
        var repo = new Mock<IProductRepository>(MockBehavior.Strict);
        var sut = new DeleteProductHandler(repo.Object);

        var existing = new Product("P", "C", 1m) { Id = 10 };
        var command = new DeleteProductCommand { Id = 10 };

        repo.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(existing);
        repo.Setup(r => r.DeleteAsync(existing)).Returns(Task.CompletedTask);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(10, result);
        repo.Verify(r => r.GetByIdAsync(command.Id), Times.Once);
        repo.Verify(r => r.DeleteAsync(existing), Times.Once);
        repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenProductNotFound()
    {
        // Arrange
        var repo = new Mock<IProductRepository>(MockBehavior.Strict);
        var sut = new DeleteProductHandler(repo.Object);

        var command = new DeleteProductCommand { Id = 999 };
        repo.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync((Product?)null);

        // Act
        var act = () => sut.Handle(command, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(act);
        Assert.Equal("Product not found", ex.Message);
        repo.Verify(r => r.GetByIdAsync(command.Id), Times.Once);
        repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRepositoryDeleteThrows()
    {
        // Arrange
        var repo = new Mock<IProductRepository>(MockBehavior.Strict);
        var sut = new DeleteProductHandler(repo.Object);

        var existing = new Product("P", "C", 1m) { Id = 10 };
        var command = new DeleteProductCommand { Id = 10 };

        repo.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(existing);
        repo.Setup(r => r.DeleteAsync(existing)).ThrowsAsync(new InvalidOperationException("boom"));

        // Act
        var act = () => sut.Handle(command, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.Equal("boom", ex.Message);
        repo.Verify(r => r.GetByIdAsync(command.Id), Times.Once);
        repo.Verify(r => r.DeleteAsync(existing), Times.Once);
        repo.VerifyNoOtherCalls();
    }
}

