using Market.Application.Features.Orders.Queries.GetOrdersByLocation;
using Market.Domain.Entities.Market;
using Moq;

namespace Market.ApplicationTest.Orders.Queries;

[TestFixture]
public class GetOrdersByLocationHandlerTests : TestBase
{
    private GetOrdersByLocationHandler _handler = null!;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        _handler = new GetOrdersByLocationHandler(MockUnitOfWork.Object, Mapper);
    }

    [Test]
    public async Task Handle_ShouldReturnOrders_WhenOrdersExistForLocation()
    {
        // Arrange
        var locationId = 10L;

        var orders = new List<Order>
        {
            CreateTestOrderWithDetails(1),
            CreateTestOrderWithDetails(2)
        };

        MockOrderRepository
            .Setup(repo => repo.GetOrdersByLocationAsync(locationId))
            .ReturnsAsync(orders);

        var query = new GetOrdersByLocationQuery(locationId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Count, Is.EqualTo(2));
            Assert.That(result.Message, Is.EqualTo("Found 2 orders for location."));
        });

        MockOrderRepository.Verify(repo => repo.GetOrdersByLocationAsync(locationId), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldReturnEmptyList_WhenNoOrdersExistForLocation()
    {
        // Arrange
        var locationId = 20L;

        MockOrderRepository
            .Setup(repo => repo.GetOrdersByLocationAsync(locationId))
            .ReturnsAsync([]);

        var query = new GetOrdersByLocationQuery(locationId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Count, Is.EqualTo(0));
            Assert.That(result.Message, Is.EqualTo("Found 0 orders for location."));
        });

        MockOrderRepository.Verify(repo => repo.GetOrdersByLocationAsync(locationId), Times.Once);
    }
}