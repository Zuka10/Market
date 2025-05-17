using Market.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace Market.API.Controllers;

/// <summary>
/// Controller for managing categories.
/// </summary>
[ApiController]
[Route("[controller]")]
public class CategoryController : ControllerBase
{
    private static readonly List<Category> categories =
    [
        new Category
        {
            Id = 1,
            Name = "Electronics",
            Description = "Devices and gadgets"
        },
        new Category
        {
            Id = 2,
            Name = "Books",
            Description = "Literature and novels"
        },
        new Category
        {
            Id = 3,
            Name = "Clothing",
            Description = "Apparel and accessories"
        },
        new Category
        {
            Id = 4,
            Name = "Home & Kitchen",
            Description = "Household items and kitchenware"
        },
    ];

    /// <summary>
    /// Gets all categories.
    /// </summary>
    /// <returns>List of categories.</returns>
    [HttpGet]
    public ActionResult<IEnumerable<Category>> GetAll()
    {
        return Ok(categories);
    }

    /// <summary>
    /// Gets a specific category by ID.
    /// </summary>
    /// <param name="id">Category ID.</param>
    /// <returns>The matching category.</returns>
    [HttpGet("{id}")]
    public ActionResult<Category> GetById(int id)
    {
        var category = categories.FirstOrDefault(c => c.Id == id);
        if (category == null)
        {
            return NotFound();
        }
        return Ok(category);
    }

    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <param name="category">Category object to create.</param>
    /// <returns>The created category.</returns>
    [HttpPost]
    public ActionResult<Category> Create([FromBody] Category category)
    {
        if (category == null)
        {
            return BadRequest();
        }

        category.Id = categories.Max(c => c.Id) + 1;
        categories.Add(category);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    /// <param name="id">ID of the category to update.</param>
    /// <param name="category">Updated category object.</param>
    /// <returns>No content.</returns>
    [HttpPut("{id}")]
    public ActionResult<Category> Update(int id, [FromBody] Category category)
    {
        if (category == null)
        {
            return BadRequest();
        }

        var existingCategory = categories.FirstOrDefault(c => c.Id == id);
        if (existingCategory == null)
        {
            return NotFound();
        }

        existingCategory.Name = category.Name;
        existingCategory.Description = category.Description;

        return NoContent();
    }

    /// <summary>
    /// Deletes an category by ID.
    /// </summary>
    /// <param name="id">ID of the category to delete.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var category = categories.FirstOrDefault(c => c.Id == id);
        if (category == null)
        {
            return NotFound();
        }

        categories.Remove(category);
        return NoContent();
    }
}