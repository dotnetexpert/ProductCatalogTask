using Products.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Products.Domain.Entities
{
    public class Product 
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public decimal Price { get; set; }

        public Product(string name, string color, decimal price)
        {
            Name = name;
            Color = color;
            Price = price;
        }

        public void Update(string name, string color, decimal price)
        {
            Name = name;
            Color = color;
            Price = price;
        }
    }
}
