using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Products.Api.Contracts.Common;
using Products.Application.Features.Products.Dtos;
using Products.Domain.Entities;
using Products.Domain.Interfaces;
using Products.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Products.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly string _filePath;

        public ProductRepository(IConfiguration configuration)
        {
            _filePath = configuration["ProductSettings:FilePath"];

            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]");
            }
        }

        public async Task<Product> AddAsync(Product product)
        {
            var products = await GetAllAsync();

            product.Id = products.Any()
                ? products.Max(x => x.Id) + 1
                : 1;

            products.Add(product);

            var json = JsonConvert.SerializeObject(products, Formatting.Indented);

            await File.WriteAllTextAsync(_filePath, json);

            return product;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            var json = await File.ReadAllTextAsync(_filePath);

           var jsonData = JsonConvert.DeserializeObject<List<Product>>(json)
                   ?? new List<Product>();

            return jsonData;
        }

        public async Task<List<Product>> GetBycolorAsync(string color)
        {
            var products = await GetAllAsync();

            return products.Where(x =>
                !string.IsNullOrWhiteSpace(x.Color) &&
                x.Color.Contains(color, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            var products = await GetAllAsync();

            return products.FirstOrDefault(x => x.Id == id);
        }
        public async Task<Product> UpdateAsync(Product product)
        {
            var products = await GetAllAsync();

            var existing = products.FirstOrDefault(x => x.Id == product.Id);

            if (existing == null)
                throw new KeyNotFoundException("Product not found");

            //  Update fields
            existing.Name = product.Name;
            existing.Color = product.Color;
            existing.Price = product.Price;

            var json = JsonConvert.SerializeObject(products, Formatting.Indented);

            await File.WriteAllTextAsync(_filePath, json);

            return existing;
        }
        public async Task<GridResult<ProductDto>> GetGridPagedListAsync(GridQuery query)
        {
            var data = await GetAllAsync();

            //  FILTER (Dynamic - all properties)
            if (query.Filter != null && query.Filter.Any())
            {
                foreach (var filter in query.Filter)
                {
                    var property = typeof(Product).GetProperty(filter.Key);

                    if (property == null) continue;

                    data = data.Where(x =>
                    {
                        var value = property.GetValue(x);

                        if (value == null) return false;

                        // Type-safe handling
                        if (property.PropertyType == typeof(string))
                        {
                            return value.ToString()!
                                .Contains(filter.Value, StringComparison.OrdinalIgnoreCase);
                        }

                        if (property.PropertyType == typeof(int) || property.PropertyType == typeof(decimal))
                        {
                            return value.ToString() == filter.Value;
                        }

                        return value.ToString()!
                            .Contains(filter.Value, StringComparison.OrdinalIgnoreCase);

                    }).ToList();
                }
            }

            // SORT
            if (!string.IsNullOrEmpty(query.Sort))
            {
                var property = typeof(Product).GetProperty(
                               query.Sort,
                               BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance
                               );

                if (property != null)
                {
                    Func<Product, object?> keySelector = x => property.GetValue(x);

                    data = query.Ascending
                        ? data.OrderBy(keySelector).ToList()
                        : data.OrderByDescending(keySelector).ToList();
                }
            }

            //  TOTAL COUNT
            var total = data.Count;

            //  PAGINATION
            if (query.PageSize != int.MaxValue)
            {
                data = data
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToList();
            }

            var mapped = data.Select(x => new ProductDto
            {
                Id = x.Id,
                Name = x.Name,
                Color = x.Color,
                Price = x.Price
            }).ToList();

            //  RESULT
            return new GridResult<ProductDto>
            {
                Data = mapped,
                Total = total,
                Page = query.Page
            };
        }

        public async Task DeleteAsync(Product product)
        {
            var products = await GetAllAsync();

            var existing = products.FirstOrDefault(x => x.Id == product.Id);

            if (existing == null)
                throw new KeyNotFoundException("Product not found");

            products.Remove(existing);

            var json = JsonConvert.SerializeObject(products, Formatting.Indented);

            await File.WriteAllTextAsync(_filePath, json);
        }
    }
}
