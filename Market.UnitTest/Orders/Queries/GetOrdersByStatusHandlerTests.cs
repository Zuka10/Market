using Market.Application.Features.Orders.Queries.GetOrdersByStatus;
using Market.Domain.Entities.Market;
using Market.Domain.Enums;
using Moq;

namespace Market.ApplicationTest.Orders.Queries;

[TestFixture]
public class GetOrdersByStatusHandlerTests : TestBase
{
    private GetOrdersByStatusHandler _handler = null!;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        _handler = new GetOrdersByStatusHandler(MockUnitOfWork.Object, Mapper);
    }

    [Test]
    public async Task Handle_ShouldReturnOrders_WhenOrdersExistWithGivenStatus()
    {
        // Arrange
        var status = OrderStatus.Completed;

        var orders = new List<Order>
        {
            CreateTestOrderWithDetails(1),
            CreateTestOrderWithDetails(2)
        };

        MockOrderRepository
            .Setup(repo => repo.GetOrdersByStatusAsync(status))
            .ReturnsAsync(orders);

        var query = new GetOrdersByStatusQuery(status);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Count, Is.EqualTo(2));
            Assert.That(result.Message, Is.EqualTo("Found 2 orders with status Completed."));
        });

        MockOrderRepository.Verify(repo => repo.GetOrdersByStatusAsync(status), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldReturnEmptyList_WhenNoOrdersExistWithGivenStatus()
    {
        // Arrange
        var status = OrderStatus.Cancelled;

        MockOrderRepository
            .Setup(repo => repo.GetOrdersByStatusAsync(status))
            .ReturnsAsync([]);

        var query = new GetOrdersByStatusQuery(status);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Count, Is.EqualTo(0));
            Assert.That(result.Message, Is.EqualTo("Found 0 orders with status Cancelled."));
        });

        MockOrderRepository.Verify(repo => repo.GetOrdersByStatusAsync(status), Times.Once);
    }
}