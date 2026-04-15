using Microsoft.Extensions.Configuration;
using Products.Api.Contracts.Common;
using Products.Domain.Entities;
using Products.Infrastructure.Repositories;

namespace Products.Api.Tests.Infrastructure.Repositories;

public class ProductRepositoryTests
{
    [Fact]
    public async Task AddAsync_ShouldAssignIncrementedId_WhenExistingProductsPresent()
    {
        // Arrange
        var filePath = CreateTempProductsFile("""
            [
              { "Id": 1, "Name": "A", "Color": "Red", "Price": 1.0 },
              { "Id": 3, "Name": "B", "Color": "Blue", "Price": 2.0 }
            ]
            """);

        var sut = CreateSut(filePath);

        var product = new Product("C", "Green", 3m);

        // Act
        var created = await sut.AddAsync(product);

        // Assert
        Assert.Equal(4, created.Id);
        var all = await sut.GetAllAsync();
        Assert.Contains(all, p => p.Id == 4 && p.Name == "C");
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowKeyNotFoundException_WhenProductDoesNotExist()
    {
        // Arrange
        var filePath = CreateTempProductsFile("[]");
        var sut = CreateSut(filePath);

        var product = new Product("X", "Y", 1m) { Id = 99 };

        // Act
        var act = () => sut.UpdateAsync(product);

        // Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(act);
        Assert.Equal("Product not found", ex.Message);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveProduct_WhenProductExists()
    {
        // Arrange
        var filePath = CreateTempProductsFile("""
            [
              { "Id": 1, "Name": "A", "Color": "Red", "Price": 1.0 },
              { "Id": 2, "Name": "B", "Color": "Blue", "Price": 2.0 }
            ]
            """);

        var sut = CreateSut(filePath);
        var product = (await sut.GetByIdAsync(2))!;

        // Act
        await sut.DeleteAsync(product);

        // Assert
        var all = await sut.GetAllAsync();
        Assert.DoesNotContain(all, p => p.Id == 2);
    }

    [Fact]
    public async Task GetGridPagedListAsync_ShouldFilterSortAndPaginate_WhenQueryProvided()
    {
        // Arrange
        var filePath = CreateTempProductsFile("""
            [
              { "Id": 1, "Name": "Zeta", "Color": "Red", "Price": 10.0 },
              { "Id": 2, "Name": "Alpha", "Color": "Red", "Price": 20.0 },
              { "Id": 3, "Name": "Beta", "Color": "Blue", "Price": 30.0 }
            ]
            """);

        var sut = CreateSut(filePath);

        var query = new GridQuery
        {
            Filter = new Dictionary<string, string> { ["Color"] = "red" },
            Sort = "Name",
            Ascending = true,
            Page = 1,
            PageSize = 1
        };

        // Act
        var result = await sut.GetGridPagedListAsync(query);

        // Assert
        Assert.Equal(2, result.Total);
        Assert.Equal(1, result.Page);
        Assert.Single(result.Data);
        Assert.Equal("Alpha", result.Data[0].Name);
    }

    private static ProductRepository CreateSut(string filePath)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ProductSettings:FilePath"] = filePath
            })
            .Build();

        return new ProductRepository(config);
    }

    private static string CreateTempProductsFile(string json)
    {
        var dir = Path.Combine(Path.GetTempPath(), "Products.Api.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);

        var path = Path.Combine(dir, "products.json");
        File.WriteAllText(path, json);
        return path;
    }
}

