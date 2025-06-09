using Market.Application.Features.Orders.Commands.UpdateOrderStatus;
using Market.Domain.Entities.Market;
using Market.Domain.Enums;
using Moq;

namespace Market.ApplicationTest.Orders.Commands;

[TestFixture]
public class UpdateOrderStatusHandlerTests : TestBase
{
    private UpdateOrderStatusHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        base.SetUp();
        _handler = new UpdateOrderStatusHandler(MockUnitOfWork.Object);
    }

    [Test]
    public async Task Handle_ShouldReturnSuccessResponse_WhenOrderExists()
    {
        // Arrange
        var orderId = 1L;
        var existingOrder = new Order
        {
            Id = orderId,
            Status = OrderStatus.Pending,
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        MockOrderRepository
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(existingOrder);

        MockOrderRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateOrderStatusCommand(
            OrderId: orderId,
            NewStatus: OrderStatus.Completed
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.True);
            Assert.That(result.Message, Is.EqualTo("Order status updated to Completed successfully."));
        });

        MockOrderRepository.Verify(r => r.GetByIdAsync(orderId), Times.Once);
        MockOrderRepository.Verify(r => r.UpdateAsync(It.Is<Order>(o =>
            o.Id == orderId &&
            o.Status == OrderStatus.Completed
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

        var command = new UpdateOrderStatusCommand(
            OrderId: orderId,
            NewStatus: OrderStatus.Completed
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