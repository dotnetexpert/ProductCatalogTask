using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Products.Api.Contracts.Common;
using Products.Application.Features.Products.Command;
using Products.Application.Features.Products.Commands;
using Products.Application.Features.Products.Dtos;
using Products.Application.Features.Products.Queries;

namespace Products.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductsController : Base.BaseController
    {
        [HttpPost("Create")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(CreateProductCommand command)
        {
            var id = await Mediator.Send(command);
            return CreatedResponse(id, "Product created successfully");
        }

        [HttpPost("get-all")]
        [ProducesResponseType(typeof(ApiResponse<GridResult<List<ProductDto>>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll([FromBody] GetProductsQuery query)
        {
            var result = await Mediator.Send(query);
            return Success(result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, UpdateProductCommand command)
        {
            command.Id = id;

            var result = await Mediator.Send(command);

            return Success(result, "Product updated successfully");
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteProductCommand { Id = id };

            var result = await Mediator.Send(command);

            return Success(result, "Product deleted successfully");
        }
    }
}
