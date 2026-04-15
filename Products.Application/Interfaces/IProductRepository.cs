using Products.Api.Contracts.Common;
using Products.Application.Features.Products.Dtos;
using Products.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Products.Domain.Interfaces
{
    public interface IProductRepository
    {
        Task<Product> AddAsync(Product product);
        Task<List<Product>> GetAllAsync();
        Task<List<Product>> GetBycolorAsync(string color);
        Task<Product?> GetByIdAsync(int id);
        Task<Product> UpdateAsync(Product product);
        Task<GridResult<ProductDto>> GetGridPagedListAsync(GridQuery query);
        Task DeleteAsync(Product product);
    }
}
