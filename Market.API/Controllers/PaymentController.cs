using Market.API.Models;
using Market.API.Models.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Market.API.Controllers;

/// <summary>
/// Controller for managing payments.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class PaymentController : ControllerBase
{
    private static readonly List<Payment> payments =
    [
         new Payment
         {
             Id = 1,
             PaymentMethod = PaymentType.CreditCard,
             Amount = 150.00m,
             PaymentDate = DateTime.UtcNow,
             Status = PaymentStatus.Completed,
             OrderId = 1
         },
         new Payment
         {
             Id = 2,
             PaymentMethod = PaymentType.Cash,
             Amount = 75.50m,
             PaymentDate = DateTime.UtcNow,
             Status = PaymentStatus.Pending,
             OrderId = 2
         },
         new Payment
         {
             Id = 3,
             PaymentMethod = PaymentType.Other,
             Amount = 200.00m,
             PaymentDate = DateTime.UtcNow,
             Status = PaymentStatus.Failed,
             OrderId = 3
         },
         new Payment
         {
             Id = 4,
             PaymentMethod = PaymentType.Cash,
             Amount = 50.00m,
             PaymentDate = DateTime.UtcNow,
             Status = PaymentStatus.Completed,
             OrderId = 4
         },
    ];

    /// <summary>
    /// Gets all payments.
    /// </summary>
    /// <returns>List of payments.</returns>
    [HttpGet]
    public ActionResult<IEnumerable<Payment>> GetAll()
    {
        return Ok(payments);
    }

    /// <summary>
    /// Gets a specific payment by ID.
    /// </summary>
    /// <param name="id">Payment ID.</param>
    /// <returns>The matching payment.</returns>
    [HttpGet("{id}")]
    public ActionResult<Payment> GetById(int id)
    {
        var payment = payments.FirstOrDefault(p => p.Id == id);
        if (payment is null)
        {
            return NotFound();
        }
        return Ok(payment);
    }

    /// <summary>
    /// Creates a new payment.
    /// </summary>
    /// <param name="payment">Payment object to create.</param>
    /// <returns>The created payment.</returns>
    [HttpPost]
    public ActionResult<Payment> Create([FromBody] Payment payment)
    {
        if (payment is null)
        {
            return BadRequest("Payment cannot be null.");
        }

        payment.Id = payments.Max(p => p.Id) + 1;
        payments.Add(payment);
        return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
    }

    /// <summary>
    /// Updates an existing payment.
    /// </summary>
    /// <param name="id">ID of the payment to update (from route).</param>
    /// <param name="payment">Updated payment object.</param>
    /// <returns>Returns 200 OK if successful, 404 if not found, or 400 if request is invalid.</returns>
    /// <response code="200">A specific payment.</response>
    /// <response code="400">Payment is null</response>
    /// <response code="404">No payment with the provided Id were found.</response>
    [HttpPut("{id}")]
    public ActionResult Update([FromRoute] int id, [FromBody] Payment payment)
    {
        if (payment is null)
        {
            return BadRequest("Payment cannot be null.");
        }

        var existingPayment = payments.FirstOrDefault(p => p.Id == id);
        if (existingPayment is null)
        {
            return NotFound();
        }

        existingPayment.PaymentMethod = payment.PaymentMethod;
        existingPayment.Amount = payment.Amount;
        existingPayment.PaymentDate = payment.PaymentDate;
        existingPayment.Status = payment.Status;

        return Ok();
    }


    /// <summary>
    /// Deletes an payment by ID.
    /// </summary>
    /// <param name="id">ID of the payment to delete.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var payment = payments.FirstOrDefault(p => p.Id == id);
        if (payment is null)
        {
            return NotFound();
        }

        payments.Remove(payment);
        return Ok();
    }
}