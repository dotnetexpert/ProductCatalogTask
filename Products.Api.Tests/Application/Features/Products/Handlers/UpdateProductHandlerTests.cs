using Moq;
using Products.Application.Features.Products.Commands;
using Products.Application.Features.Products.Handlers;
using Products.Domain.Entities;
using Products.Domain.Interfaces;

namespace Products.Api.Tests.Application.Features.Products.Handlers;

public class UpdateProductHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnProductId_WhenProductExists()
    {
        // Arrange
        var repo = new Mock<IProductRepository>(MockBehavior.Strict);
        var sut = new UpdateProductHandler(repo.Object);

        var existing = new Product("Old", "Blue", 10m) { Id = 7 };

        var command = new UpdateProductCommand
        {
            Id = 7,
            Name = "New",
            Color = "Red",
            Price = 20m
        };

        repo.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(existing);
        repo.Setup(r => r.UpdateAsync(existing)).ReturnsAsync(existing);

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(7, result);
        Assert.Equal("New", existing.Name);
        Assert.Equal("Red", existing.Color);
        Assert.Equal(20m, existing.Price);
        repo.Verify(r => r.GetByIdAsync(command.Id), Times.Once);
        repo.Verify(r => r.UpdateAsync(existing), Times.Once);
        repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenProductNotFound()
    {
        // Arrange
        var repo = new Mock<IProductRepository>(MockBehavior.Strict);
        var sut = new UpdateProductHandler(repo.Object);

        var command = new UpdateProductCommand
        {
            Id = 999,
            Name = "New",
            Color = "Red",
            Price = 20m
        };

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
    public async Task Handle_ShouldThrowException_WhenRepositoryUpdateThrows()
    {
        // Arrange
        var repo = new Mock<IProductRepository>(MockBehavior.Strict);
        var sut = new UpdateProductHandler(repo.Object);

        var existing = new Product("Old", "Blue", 10m) { Id = 7 };

        var command = new UpdateProductCommand
        {
            Id = 7,
            Name = "New",
            Color = "Red",
            Price = 20m
        };

        repo.Setup(r => r.GetByIdAsync(command.Id)).ReturnsAsync(existing);
        repo.Setup(r => r.UpdateAsync(existing)).ThrowsAsync(new InvalidOperationException("boom"));

        // Act
        var act = () => sut.Handle(command, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.Equal("boom", ex.Message);
        repo.Verify(r => r.GetByIdAsync(command.Id), Times.Once);
        repo.Verify(r => r.UpdateAsync(existing), Times.Once);
        repo.VerifyNoOtherCalls();
    }
}

