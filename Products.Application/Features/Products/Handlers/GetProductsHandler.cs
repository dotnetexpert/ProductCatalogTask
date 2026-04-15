using MediatR;
using Products.Api.Contracts.Common;
using Products.Application.Common.Interfaces;
using Products.Application.Features.Products.Dtos;
using Products.Application.Features.Products.Queries;
using Products.Domain.Entities;
using Products.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Products.Application.Features.Products.Handlers
{
    public class GetProductsHandler : IRequestHandler<GetProductsQuery, GridResult<ProductDto>>
    {
        private readonly IProductRepository _repo;
        private readonly IMapperService _mapper;

        public GetProductsHandler(IProductRepository repo, IMapperService mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<GridResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var result = await _repo.GetGridPagedListAsync(request);
  
            return result; 
        }
    }
}
