using Market.Application.Features.Orders.Commands.CancelOrder;
using Market.Domain.Entities.Market;
using Moq;

namespace Market.ApplicationTest.Orders.Commands;

[TestFixture]
public class CancelOrderHandlerTests : TestBase
{
    private CancelOrderHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        base.SetUp();
        _handler = new CancelOrderHandler(MockUnitOfWork.Object);
    }

    [Test]
    public async Task Handle_ShouldReturnSuccessResponse_WhenOrderExists()
    {
        // Arrange
        var orderId = 1L;
        var existingOrder = new Order { Id = orderId };

        MockOrderRepository
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(existingOrder);

        MockOrderRepository
            .Setup(r => r.DeleteAsync(orderId))
            .Returns(Task.CompletedTask);

        var command = new CancelOrderCommand(OrderId: orderId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.True);
            Assert.That(result.Message, Is.EqualTo("Order cancelled successfully."));
        });

        MockOrderRepository.Verify(r => r.GetByIdAsync(orderId), Times.Once);
        MockOrderRepository.Verify(r => r.DeleteAsync(orderId), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldReturnFailureResponse_WhenOrderDoesNotExist()
    {
        // Arrange
        var orderId = 999L;

        MockOrderRepository
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync((Order?)null);

        var command = new CancelOrderCommand(OrderId: orderId);

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
        MockOrderRepository.Verify(r => r.DeleteAsync(It.IsAny<long>()), Times.Never);
    }
}