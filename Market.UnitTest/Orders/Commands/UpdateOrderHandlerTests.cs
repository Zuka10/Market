using Market.Application.Features.Orders.Commands.UpdateOrder;
using Market.Domain.Entities.Market;
using Market.Domain.Enums;
using Moq;

namespace Market.ApplicationTest.Orders.Commands;

[TestFixture]
public class UpdateOrderHandlerTests : TestBase
{
    private UpdateOrderHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        base.SetUp();
        _handler = new UpdateOrderHandler(MockUnitOfWork.Object);
    }

    [Test]
    public async Task Handle_ShouldReturnSuccessResponse_WhenOrderExists()
    {
        // Arrange
        var orderId = 1L;
        var existingOrder = new Order
        {
            Id = orderId,
            OrderNumber = "OldOrder123",
            OrderDate = DateTime.UtcNow.AddDays(-1),
            Total = 100,
            SubTotal = 90,
            TotalCommission = 10,
            Status = OrderStatus.Pending,
            LocationId = 5,
            DiscountId = null,
            DiscountAmount = 0,
            UserId = 10,
            CustomerName = "John Doe",
            CustomerPhone = "123456789",
            Notes = "Old notes",
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        MockOrderRepository
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(existingOrder);

        MockOrderRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateOrderCommand(
            OrderId: orderId,
            OrderNumber: "NewOrder456",
            OrderDate: DateTime.UtcNow,
            Total: 150,
            SubTotal: 140,
            TotalCommission: 15,
            Status: OrderStatus.Completed,
            LocationId: 7,
            DiscountId: 3,
            DiscountAmount: 10,
            UserId: 20,
            CustomerName: "Jane Smith",
            CustomerPhone: "987654321",
            Notes: "Updated notes"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.True);
            Assert.That(result.Message, Is.EqualTo("Order updated successfully."));
        });

        MockOrderRepository.Verify(r => r.GetByIdAsync(orderId), Times.Once);
        MockOrderRepository.Verify(r => r.UpdateAsync(It.Is<Order>(o =>
            o.Id == orderId &&
            o.OrderNumber == "NewOrder456" &&
            o.Total == 150 &&
            o.Status == OrderStatus.Completed &&
            o.CustomerName == "Jane Smith"
        )), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldReturnFailureResponse_WhenOrderDoesNotExist()
    {
        // Arrange
        var orderId = 999L;

        MockOrderRepository
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync((Order?)null);

        var command = new UpdateOrderCommand(
            OrderId: orderId,
            OrderNumber: "AnyOrder",
            OrderDate: DateTime.UtcNow,
            Total: 0,
            SubTotal: 0,
            TotalCommission: 0,
            Status: OrderStatus.Pending,
            LocationId: 1,
            DiscountId: null,
            DiscountAmount: 0,
            UserId: 1,
            CustomerName: null,
            CustomerPhone: null,
            Notes: null
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors, Does.Contain("Order not found."));
        });

        MockOrderRepository.Verify(r => r.GetByIdAsync(orderId), Times.Once);
        MockOrderRepository.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Never);
    }
}