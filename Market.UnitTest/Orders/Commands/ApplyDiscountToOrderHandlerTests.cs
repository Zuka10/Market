using Market.Application.Features.Orders.Commands.ApplyDiscountToOrder;
using Market.Domain.Entities.Market;
using Moq;

namespace Market.ApplicationTest.Orders.Commands;

[TestFixture]
public class ApplyDiscountToOrderHandlerTests : TestBase
{
    private ApplyDiscountToOrderHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        base.SetUp();
        _handler = new ApplyDiscountToOrderHandler(MockUnitOfWork.Object);
    }

    [Test]
    public async Task Handle_ShouldReturnSuccessResponse_WhenDiscountAppliedSuccessfully()
    {
        // Arrange
        var orderId = 1L;
        var existingOrder = new Order
        {
            Id = orderId,
            SubTotal = 100m,
            DiscountAmount = 0m,
            Total = 100m,
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        MockOrderRepository
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(existingOrder);

        MockOrderRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        var command = new ApplyDiscountToOrderCommand(
            OrderId: orderId,
            DiscountId: 5,
            DiscountAmount: 20m
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.True);
            Assert.That(result.Message, Is.EqualTo("Discount applied to order successfully."));
            Assert.That(existingOrder.DiscountAmount, Is.EqualTo(20m));
            Assert.That(existingOrder.Total, Is.EqualTo(80m));
            Assert.That(existingOrder.DiscountId, Is.EqualTo(5));
        });

        MockOrderRepository.Verify(r => r.GetByIdAsync(orderId), Times.Once);
        MockOrderRepository.Verify(r => r.UpdateAsync(It.Is<Order>(o =>
            o.Id == orderId &&
            o.DiscountAmount == 20m &&
            o.Total == 80m &&
            o.DiscountId == 5
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

        var command = new ApplyDiscountToOrderCommand(
            OrderId: orderId,
            DiscountId: 1,
            DiscountAmount: 10m
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

    [Test]
    public async Task Handle_ShouldReturnFailureResponse_WhenDiscountAmountExceedsSubTotal()
    {
        // Arrange
        var orderId = 2L;
        var existingOrder = new Order
        {
            Id = orderId,
            SubTotal = 50m,
            DiscountAmount = 0m,
            Total = 50m,
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        MockOrderRepository
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(existingOrder);

        var command = new ApplyDiscountToOrderCommand(
            OrderId: orderId,
            DiscountId: 2,
            DiscountAmount: 60m // exceeds subtotal
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors, Does.Contain("Discount amount cannot exceed order subtotal."));
        });

        MockOrderRepository.Verify(r => r.GetByIdAsync(orderId), Times.Once);
        MockOrderRepository.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Never);
    }
}