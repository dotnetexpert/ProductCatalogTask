using AutoMapper;
using Products.Application.Features.Products.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Products.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            #region ProductMapping
            CreateMap<Domain.Entities.Product, ProductDto>();

            #endregion
        }
    }
}
