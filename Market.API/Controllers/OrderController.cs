using Market.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace Market.API.Controllers;

/// <summary>
/// Controller for managing orders.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class OrderController : ControllerBase
{
    private static readonly List<Order> _orders =
    [
        new Order
        {
            Id = 1,
            CustomerName = "John Doe",
            OrderDate = DateTime.Now,
            OrderDetails =
            [
                new OrderDetail
                {
                    Id = 1,
                    ProductId = 1,
                    Quantity = 2,
                    UnitPrice = 999.99m
                },
                new OrderDetail
                {
                    Id = 2,
                    ProductId = 3,
                    Quantity = 1,
                    UnitPrice = 19.99m
                }
            ]
        },
        new Order
        {
            Id = 2,
            CustomerName = "Jane Smith",
            OrderDate = DateTime.Now,
            OrderDetails =
            [
                new OrderDetail
                {
                    Id = 3,
                    ProductId = 2,
                    Quantity = 1,
                    UnitPrice = 699.99m
                },
                new OrderDetail
                {
                    Id = 4,
                    ProductId = 4,
                    Quantity = 3,
                    UnitPrice = 15.99m
                }
            ]
        },
        new Order
        {
            Id = 3,
            CustomerName = "Alice Johnson",
            OrderDate = DateTime.Now,
            OrderDetails =
            [
                new OrderDetail
                {
                    Id = 5,
                    ProductId = 5,
                    Quantity = 1,
                    UnitPrice = 49.99m
                }
            ]
        }
    ];

    /// <summary>
    /// Gets all orders.
    /// </summary>
    /// <returns>List of orders.</returns>
    [HttpGet]
    public ActionResult<IEnumerable<Order>> GetAll()
    {
        return Ok(_orders);
    }

    /// <summary>
    /// Gets a specific order by ID.
    /// </summary>
    /// <param name="id">Order ID.</param>
    /// <returns>The matching order.</returns>
    [HttpGet("{id}")]
    public ActionResult<Order> GetById(int id)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order is null)
        {
            return NotFound();
        }
        return Ok(order);
    }

    /// <summary>
    /// Creates a new order.
    /// </summary>
    /// <param name="order">Order object to create.</param>
    /// <returns>The created order.</returns>
    [HttpPost]
    public ActionResult<Order> Create([FromBody] Order order)
    {
        if (order is null)
        {
            return BadRequest();
        }

        order.Id = _orders.Max(o => o.Id) + 1;
        _orders.Add(order);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    /// <summary>
    /// Updates an existing order.
    /// </summary>
    /// <param name="id">ID of the order to update (from route).</param>
    /// <param name="order">Updated order object.</param>
    /// <returns>Returns 200 OK if successful, 404 if not found, or 400 if request is invalid.</returns>
    /// <response code="200">A specific order.</response>
    /// <response code="400">Order is null</response>
    /// <response code="404">No order with the provided Id were found.</response>
    [HttpPut("{id}")]
    public ActionResult<Order> Update([FromRoute] int id, [FromBody] Order order)
    {
        if (order is null)
        {
            return BadRequest();
        }

        var existingOrder = _orders.FirstOrDefault(o => o.Id == id);
        if (existingOrder is null)
        {
            return NotFound();
        }

        existingOrder.CustomerName = order.CustomerName;
        existingOrder.OrderDate = order.OrderDate;
        existingOrder.OrderDetails = order.OrderDetails;

        return Ok();
    }

    /// <summary>
    /// Deletes an order by ID.
    /// </summary>
    /// <param name="id">ID of the order to delete.</param>
    /// <returns>Ok.</returns>
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order is null)
        {
            return NotFound();
        }

        _orders.Remove(order);
        return Ok();
    }
}