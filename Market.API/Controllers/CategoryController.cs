using Market.Application.Common.Models;
using Market.Application.DTOs.Market;
using Market.Application.Features.Categories.Command.CreateCategory;
using Market.Application.Features.Categories.Command.DeleteCategory;
using Market.Application.Features.Categories.Command.UpdateCategory;
using Market.Application.Features.Categories.Queries.GetCategories;
using Market.Application.Features.Categories.Queries.GetCategoryById;
using Market.Application.Features.Categories.Queries.SearchCategories;
using Market.Application.Features.Products.Queries.GetProductsByCategory;
using Market.Domain.Filters;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Market.API.Controllers;

/// <summary>
/// Controller for managing categories.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class CategoryController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Retrieves all categories
    /// </summary>
    /// <param name="query">Category search filters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of categories</returns>
    [HttpGet]
    public async Task<ActionResult<BaseResponse<PagedResult<CategoryDto>>>> GetAll(
        [FromQuery] GetCategoriesQuery query,
        CancellationToken cancellationToken = default)
    {
        var categories = await _mediator.Send(query, cancellationToken);
        return Ok(categories);
    }

    /// <summary>
    /// Retrieves a specific category by its ID
    /// </summary>
    /// <param name="id">The category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The category details</returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCategoryById(
        int id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCategoryByIdQuery(Id: id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new category
    /// </summary>
    /// <param name="command">Category creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created category</returns>
    [HttpPost]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing category
    /// </summary>
    /// <param name="id">The category ID to update</param>
    /// <param name="command">Category update details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated category</returns>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCategory(
        int id,
        [FromBody] UpdateCategoryCommand command,
        CancellationToken cancellationToken = default)
    {
        var updateCommand = command with { CategoryId = id };
        var result = await _mediator.Send(updateCommand, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Deletes a category
    /// </summary>
    /// <param name="id">The category ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion confirmation</returns>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategory(
        int id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteCategoryCommand(CategoryId: id);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Search categories by various criteria
    /// </summary>
    /// <param name="query">Search criteria</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of categories matching the search criteria</returns>
    [HttpGet("search")]
    public async Task<IActionResult> SearchCategories(
        [FromQuery] SearchCategoriesQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves products in a specific category
    /// </summary>
    /// <param name="id">The category ID</param>
    /// <param name="isAvaliable">Filter by availability status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of products in the specified category</returns>
    [HttpGet("{id:int}/products")]
    public async Task<IActionResult> GetProductsInCategory(
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
}