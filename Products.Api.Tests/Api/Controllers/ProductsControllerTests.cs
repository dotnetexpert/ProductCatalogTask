using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Products.Api.Contracts.Common;
using Products.Api.Controllers;
using Products.Api.Tests.TestUtils;
using Products.Application.Features.Products.Command;
using Products.Application.Features.Products.Commands;
using Products.Application.Features.Products.Dtos;
using Products.Application.Features.Products.Queries;

namespace Products.Api.Tests.Api.Controllers;

public class ProductsControllerTests
{
    [Fact]
    public async Task Create_ShouldReturnCreatedResponse_WhenMediatorReturnsId()
    {
        // Arrange
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        var sut = ControllerTestFactory.CreateController<ProductsController>(mediator.Object);

        var command = new CreateProductCommand
        {
            Name = "A",
            Color = "Red",
            Price = 1m
        };

        mediator.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        // Act
        var result = await sut.Create(command);

        // Assert
        var created = Assert.IsType<CreatedResult>(result);
        Assert.Equal(201, created.StatusCode);

        var payload = Assert.IsType<ApiResponse<int>>(created.Value);
        Assert.True(payload.Success);
        Assert.Equal(10, payload.Data);
        Assert.Equal("Product created successfully", payload.Message);

        mediator.Verify(m => m.Send(command, It.IsAny<CancellationToken>()), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkResponse_WhenMediatorReturnsGridResult()
    {
        // Arrange
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        var sut = ControllerTestFactory.CreateController<ProductsController>(mediator.Object);

        var query = new GetProductsQuery { Page = 1, PageSize = 10 };

        var grid = new GridResult<ProductDto>
        {
            Data = new List<ProductDto> { new ProductDto { Id = 1, Name = "A", Color = "Red", Price = 1m } },
            Total = 1,
            Page = 1
        };

        mediator.Setup(m => m.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(grid);

        // Act
        var result = await sut.GetAll(query);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<ApiResponse<GridResult<ProductDto>>>(ok.Value);
        Assert.True(payload.Success);
        Assert.Same(grid, payload.Data);

        mediator.Verify(m => m.Send(query, It.IsAny<CancellationToken>()), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_ShouldSetIdAndReturnOkResponse_WhenMediatorReturnsId()
    {
        // Arrange
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        var sut = ControllerTestFactory.CreateController<ProductsController>(mediator.Object);

        var routeId = 5;
        var command = new UpdateProductCommand
        {
            Id = 0,
            Name = "B",
            Color = "Blue",
            Price = 2m
        };

        mediator.Setup(m => m.Send(It.Is<UpdateProductCommand>(c =>
                c.Id == routeId &&
                c.Name == command.Name &&
                c.Color == command.Color &&
                c.Price == command.Price),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(routeId);

        // Act
        var result = await sut.Update(routeId, command);

        // Assert
        Assert.Equal(routeId, command.Id);

        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<ApiResponse<int>>(ok.Value);
        Assert.True(payload.Success);
        Assert.Equal(routeId, payload.Data);
        Assert.Equal("Product updated successfully", payload.Message);

        mediator.Verify(m => m.Send(It.IsAny<UpdateProductCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Delete_ShouldSendDeleteCommandAndReturnOkResponse_WhenMediatorReturnsId()
    {
        // Arrange
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        var sut = ControllerTestFactory.CreateController<ProductsController>(mediator.Object);

        var id = 9;

        mediator.Setup(m => m.Send(It.Is<DeleteProductCommand>(c => c.Id == id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(id);

        // Act
        var result = await sut.Delete(id);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var payload = Assert.IsType<ApiResponse<int>>(ok.Value);
        Assert.True(payload.Success);
        Assert.Equal(id, payload.Data);
        Assert.Equal("Product deleted successfully", payload.Message);

        mediator.Verify(m => m.Send(It.Is<DeleteProductCommand>(c => c.Id == id), It.IsAny<CancellationToken>()), Times.Once);
        mediator.VerifyNoOtherCalls();
    }
}

