using MediatR;
using Products.Api.Contracts.Common;
using Products.Application.Features.Products.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Products.Application.Features.Products.Queries
{
    public class GetProductsQuery : GridQuery , IRequest<GridResult<ProductDto>>
    {
    }
}
