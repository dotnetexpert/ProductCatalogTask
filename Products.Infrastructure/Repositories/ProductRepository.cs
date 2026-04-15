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

            if (query.Filter != null && query.Filter.TryGetValue("search", out var searchValue))
            {
                var search = searchValue?.Trim();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    data = data.Where(x =>
                        (!string.IsNullOrEmpty(x.Name) && x.Name.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(x.Color) && x.Color.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        x.Price.ToString().Contains(search)
                    ).ToList();
                }
            }

            // Always sort by Id DESC
            data = data.OrderByDescending(x => x.Id).ToList();

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
