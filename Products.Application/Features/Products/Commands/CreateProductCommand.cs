using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Products.Application.Features.Products.Command
{
    public class CreateProductCommand : IRequest<int>
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public decimal Price { get; set; }
    }
}
