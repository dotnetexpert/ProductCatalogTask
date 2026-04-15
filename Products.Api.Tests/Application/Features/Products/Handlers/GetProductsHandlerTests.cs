using Moq;
using Products.Api.Contracts.Common;
using Products.Application.Common.Interfaces;
using Products.Application.Features.Products.Dtos;
using Products.Application.Features.Products.Handlers;
using Products.Application.Features.Products.Queries;
using Products.Domain.Interfaces;

namespace Products.Api.Tests.Application.Features.Products.Handlers;

public class GetProductsHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnGridResult_WhenRepositoryReturnsData()
    {
        // Arrange
        var repo = new Mock<IProductRepository>(MockBehavior.Strict);
        var mapper = new Mock<IMapperService>(MockBehavior.Strict);
        var sut = new GetProductsHandler(repo.Object, mapper.Object);

        var query = new GetProductsQuery
        {
            Page = 1,
            PageSize = 10
        };

        var expected = new GridResult<ProductDto>
        {
            Data = new List<ProductDto>
            {
                new ProductDto { Id = 1, Name = "A", Color = "Red", Price = 1m }
            },
            Total = 1,
            Page = 1
        };

        repo.Setup(r => r.GetGridPagedListAsync(query)).ReturnsAsync(expected);

        // Act
        var result = await sut.Handle(query, CancellationToken.None);

        // Assert
        Assert.Same(expected, result);
        repo.Verify(r => r.GetGridPagedListAsync(query), Times.Once);
        repo.VerifyNoOtherCalls();
        mapper.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenRepositoryThrows()
    {
        // Arrange
        var repo = new Mock<IProductRepository>(MockBehavior.Strict);
        var mapper = new Mock<IMapperService>(MockBehavior.Strict);
        var sut = new GetProductsHandler(repo.Object, mapper.Object);

        var query = new GetProductsQuery();

        repo.Setup(r => r.GetGridPagedListAsync(query))
            .ThrowsAsync(new InvalidOperationException("boom"));

        // Act
        var act = () => sut.Handle(query, CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(act);
        Assert.Equal("boom", ex.Message);
        repo.Verify(r => r.GetGridPagedListAsync(query), Times.Once);
        repo.VerifyNoOtherCalls();
        mapper.VerifyNoOtherCalls();
    }
}

