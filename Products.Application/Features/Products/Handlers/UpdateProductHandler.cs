using MediatR;
using Products.Application.Features.Products.Commands;
using Products.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Products.Application.Features.Products.Handlers
{
    public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, int>
    {
        private readonly IProductRepository _repo;

        public UpdateProductHandler(IProductRepository repo)
        {
            _repo = repo;
        }

        public async Task<int> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _repo.GetByIdAsync(request.Id);

            if (product == null)
                throw new KeyNotFoundException("Product not found");

            product.Update(request.Name, request.Color, request.Price);

            await _repo.UpdateAsync(product); 

            return product.Id;
        }
    }
}
