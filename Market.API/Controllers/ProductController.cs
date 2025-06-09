using Market.API.Models;
using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Application.Features.Products.Commands.CreateProduct;
using Market.Application.Features.Products.Commands.DeleteProduct;
using Market.Application.Features.Products.Commands.UpdateProduct;
using Market.Application.Features.Products.Queries.GetProductById;
using Market.Application.Features.Products.Queries.GetProducts;
using Market.Domain.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;

namespace Market.API.Controllers;

/// <summary>
/// Controller for managing products.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ProductController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Gets all products with optional filtering and pagination.
    /// </summary>
    /// <returns>Paged list of products</returns>
    [HttpGet]
    public async Task<ActionResult<BaseResponse<PagedResult<ProductDto>>>> GetAll([FromQuery] GetProductsQuery query)
    {
        var products = await _mediator.Send(query);
        return Ok(products);
    }

    /// <summary>
    /// Gets a specific product by ID.
    /// </summary>
    /// <param name="id">Product ID.</param>
    /// <returns>The matching product.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var product = await _mediator.Send(new GetProductByIdQuery(id));
        return Ok(product);
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="command">Product object to create.</param>
    /// <returns>The created product.</returns>
    [HttpPost]
    public ActionResult<Product> Create([FromBody] CreateProductCommand command)
    {
        var product = _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">ID of the product to update (from route).</param>
    /// <param name="command">Updated product object.</param>
    /// <returns>Returns 200 OK if successful, 404 if not found, or 400 if request is invalid.</returns>
    /// <response code="200">A specific category.</response>
    /// <response code="400">Category is null</response>
    /// <response code="404">No category with the provided Id were found.</response>
    [HttpPut("{id}")]
    public async Task<ActionResult<Product>> Update([FromRoute] int id, [FromBody] UpdateProductCommand command)
    {
        var response = await _mediator.Send(command);
        return Ok(response);
    }

    /// <summary>
    /// Deletes an product by ID.
    /// </summary>
    /// <param name="id">ID of the product to delete.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(void), 200)]
    public async Task<ActionResult> Delete(int id)
    {
        await _mediator.Send(new DeleteProductCommand(id));
        return NoContent();
    }
}