using Market.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace Market.API.Controllers;

/// <summary>
/// Controller for managing products.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private static readonly List<Product> _products =
    [
        new Product
        {
            Id = 1,
            Name = "Laptop",
            Description = "High-performance laptop",
            Price = 999.99m,
            Stock = 10,
            CategoryId = 1,
        },
        new Product
        {
            Id = 2,
            Name = "Smartphone",
            Description = "Latest model smartphone",
            Price = 699.99m,
            Stock = 20,
            CategoryId = 1,
        },
        new Product
        {
            Id = 3,
            Name = "Novel",
            Description = "Bestselling novel",
            Price = 19.99m,
            Stock = 50,
            CategoryId = 2,
        },
        new Product
        {
            Id = 4,
            Name = "T-shirt",
            Description = "Comfortable cotton t-shirt",
            Price = 15.99m,
            Stock = 100,
            CategoryId = 3,
        },
        new Product
        {
            Id = 5,
            Name = "Blender",
            Description = "High-speed blender",
            Price = 49.99m,
            Stock = 30,
            CategoryId = 4,
        },
    ];

    /// <summary>
    /// Gets all products.
    /// </summary>
    /// <returns>List of products.</returns>
    [HttpGet]
    public ActionResult<IEnumerable<Product>> GetAll()
    {
        return Ok(_products);
    }

    /// <summary>
    /// Gets a specific product by ID.
    /// </summary>
    /// <param name="id">Product ID.</param>
    /// <returns>The matching product.</returns>
    [HttpGet("{id}")]
    public ActionResult<Product> GetById(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product is null)
        {
            return NotFound();
        }
        return Ok(product);
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="product">Product object to create.</param>
    /// <returns>The created product.</returns>
    [HttpPost]
    public ActionResult<Product> Create([FromBody] Product product)
    {
        if (product is null)
        {
            return BadRequest();
        }

        product.Id = _products.Max(p => p.Id) + 1;
        _products.Add(product);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">ID of the product to update (from route).</param>
    /// <param name="product">Updated product object.</param>
    /// <returns>Returns 200 OK if successful, 404 if not found, or 400 if request is invalid.</returns>
    /// <response code="200">A specific category.</response>
    /// <response code="400">Category is null</response>
    /// <response code="404">No category with the provided Id were found.</response>
    [HttpPut("{id}")]
    public ActionResult<Product> Update([FromRoute] int id, [FromBody] Product product)
    {
        if (product is null)
        {
            return BadRequest();
        }

        var existingProduct = _products.FirstOrDefault(p => p.Id == id);
        if (existingProduct is null)
        {
            return NotFound();
        }

        existingProduct.Name = product.Name;
        existingProduct.Description = product.Description;
        existingProduct.Price = product.Price;
        existingProduct.Stock = product.Stock;
        existingProduct.CategoryId = product.CategoryId;

        return Ok();
    }

    /// <summary>
    /// Deletes an product by ID.
    /// </summary>
    /// <param name="id">ID of the product to delete.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product is null)
        {
            return NotFound();
        }

        _products.Remove(product);
        return Ok();
    }
}