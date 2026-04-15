using Moq;
using Products.Application.Features.Products.Command;
using Products.Application.Features.Products.Handlers;
using Products.Domain.Entities;
using Products.Domain.Interfaces;

namespace Products.Api.Tests.Application.Features.Products.Handlers;

public class CreateProductHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnProductId_WhenValidRequest()
    {
        // Arrange
        var repo = new Mock<IProductRepository>(MockBehavior.Strict);
        var sut = new CreateProductHandler(repo.Object);

        var command = new CreateProductCommand
        {
            Name = "Phone",
            Color = "Black",
            Price = 999.99m
        };

        repo.Setup(r => r.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product p) =>
            {
                p.Id = 123;
                return p;
            });

        // Act
        var result = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(123, result);
        repo.Verify(r => r.AddAsync(It.Is<Product>(p =>
            p.Name == command.Name &&
            p.Color == command.Color &&
            p.Price == command.Price)), Times.Once);
        repo.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRepositoryThrows()
    {
        // Arrange
        var repo = new Mock<IProductRepository>(MockBehavior.Strict);
        var sut = new CreateProductHandler(repo.Object);

        var command = new CreateProductCommand
        {
            Name = "Phone",
            Color = "Black",
            Price = 999.99m
        };

        repo.Setup(r => r.AddAsync(It.IsAny<Product>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        // Act
        var act = () => sut.Handle(command, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.Equal("boom", ex.Message);
        repo.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
        repo.VerifyNoOtherCalls();
    }
}

