using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Application.Features.Products.Commands.ActivateProduct;
using Market.Application.Features.Products.Commands.CreateProduct;
using Market.Application.Features.Products.Commands.DeactivateProduct;
using Market.Application.Features.Products.Commands.DeleteProduct;
using Market.Application.Features.Products.Commands.UpdateProduct;
using Market.Application.Features.Products.Commands.UpdateProductStock;
using Market.Application.Features.Products.Queries.GetLowStockProducts;
using Market.Application.Features.Products.Queries.GetOutOfStockProducts;
using Market.Application.Features.Products.Queries.GetProductById;
using Market.Application.Features.Products.Queries.GetProducts;
using Market.Application.Features.Products.Queries.GetProductsByCategory;
using Market.Application.Features.Products.Queries.GetProductsByLocation;
using Market.Domain.Filters;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Market.API.Controllers;

/// <summary>
/// Controller for managing products.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class ProductController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Retrieves a paginated and filtered list of products
    /// </summary>
    /// <param name="query">Product search filters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of products</returns>
    [HttpGet]
    public async Task<ActionResult<BaseResponse<PagedResult<ProductDto>>>> GetAll(
        [FromQuery] GetProductsQuery query,
        CancellationToken cancellationToken = default)
    {
        var products = await _mediator.Send(query, cancellationToken);
        return Ok(products);
    }

    /// <summary>
    /// Retrieves a specific product by its ID
    /// </summary>
    /// <param name="id">The product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The product details</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProductById(
        int id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductByIdQuery(Id: id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="command">Product creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created product</returns>
    [HttpPost]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing product
    /// </summary>
    /// <param name="id">The product ID to update</param>
    /// <param name="command">Product update details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated product</returns>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProduct(
        int id,
        [FromBody] UpdateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var updateCommand = command with { ProductId = id };
        var result = await _mediator.Send(updateCommand, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deletes a product (soft or hard delete based on system configuration)
    /// </summary>
    /// <param name="id">The product ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion confirmation</returns>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(
        int id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteProductCommand(ProductId: id);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deactivates a product (makes it unavailable for purchase)
    /// </summary>
    /// <param name="id">The product ID to deactivate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deactivation confirmation</returns>
    [HttpPatch("{id:int}/deactivate")]
    public async Task<IActionResult> DeactivateProduct(
        int id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeactivateProductCommand(ProductId: id);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Activates a product (makes it available for purchase)
    /// </summary>
    /// <param name="id">The product ID to activate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Activation confirmation</returns>
    [HttpPatch("{id:int}/activate")]
    public async Task<IActionResult> ActivateProduct(
        int id,
        CancellationToken cancellationToken = default)
    {
        var command = new ActivateProductCommand(ProductId: id);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates the stock quantity for a specific product
    /// </summary>
    /// <param name="id">The product ID</param>
    /// <param name="newStock">New stock</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated product with new stock quantity</returns>
    [HttpPatch("{id:int}/stock")]
    public async Task<IActionResult> UpdateProductStock(
        int id,
        [FromBody] int newStock,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateProductStockCommand(
            ProductId: id,
            NewStock: newStock);

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves products by category
    /// </summary>
    /// <param name="id">The category ID</param>
    /// <param name="isAvaliable">Is product avaliable</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of products in the specified category</returns>
    [HttpGet("category/{id:int}")]
    public async Task<IActionResult> GetProductsByCategory(
        int id,
        [FromQuery] bool isAvaliable = true,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductsByCategoryQuery(
            CategoryId: id,
            IsAvaliable: isAvaliable);

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves products by location
    /// </summary>
    /// <param name="id">The location ID</param>
    /// <param name="IsAvaliable">Is product avaliable</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of products in the specified location</returns>
    [HttpGet("location/{id:int}")]
    public async Task<IActionResult> GetProductsByLocation(
        int id,
        [FromQuery] bool IsAvaliable = true,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductsByLocationQuery(
            LocationId: id,
            IsAvailable: IsAvaliable);

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves products with low stock levels
    /// </summary>
    /// <param name="threshold">Stock threshold (if not provided, uses system default)</param>
    /// <param name="locationId">ID of location</param>
    /// <param name="categoryId">ID of category)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of products with low stock</returns>
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStockProducts(
        [FromQuery] int threshold,
        [FromQuery] int? locationId,
        [FromQuery] int? categoryId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLowStockProductsQuery(
            Threshold: threshold,
            LocationId: locationId,
            CategoryId: categoryId);

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves products that are out of stock
    /// </summary>
    /// <param name="locationId">ID of location</param>
    /// <param name="categoryId">ID of category</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>list of out of stock products</returns>
    [HttpGet("out-of-stock")]
    public async Task<IActionResult> GetOutOfStockProducts(
        [FromQuery] int? locationId,
        [FromQuery] int? categoryId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOutOfStockProductsQuery(
            LocationId: locationId,
            CategoryId: categoryId);

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}