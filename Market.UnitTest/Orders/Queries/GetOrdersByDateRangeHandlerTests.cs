using Market.Application.Features.Orders.Queries.GetOrdersByDateRange;
using Market.Domain.Entities.Market;
using Moq;

namespace Market.ApplicationTest.Orders.Queries;

[TestFixture]
public class GetOrdersByDateRangeHandlerTests : TestBase
{
    private GetOrdersByDateRangeHandler _handler = null!;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        _handler = new GetOrdersByDateRangeHandler(MockUnitOfWork.Object, Mapper);
    }

    [Test]
    public async Task Handle_ShouldReturnOrders_WhenOrdersExistInDateRange()
    {
        // Arrange
        var startDate = new DateTime(2024, 01, 01, 0, 0, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2024, 12, 31, 0, 0, 0, 0, 0, DateTimeKind.Utc);

        var orders = new List<Order>
        {
            CreateTestOrderWithDetails(1),
            CreateTestOrderWithDetails(2)
        };

        MockOrderRepository
            .Setup(repo => repo.GetOrdersByDateRangeAsync(startDate, endDate))
            .ReturnsAsync(orders);

        var query = new GetOrdersByDateRangeQuery(startDate, endDate);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Count, Is.EqualTo(2));
            Assert.That(result.Message, Is.EqualTo("Found 2 orders in date range."));
        });

        MockOrderRepository.Verify(repo => repo.GetOrdersByDateRangeAsync(startDate, endDate), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldReturnEmptyList_WhenNoOrdersInDateRange()
    {
        // Arrange
        var startDate = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2025, 01, 31, 0, 0, 0, DateTimeKind.Utc);

        MockOrderRepository
            .Setup(repo => repo.GetOrdersByDateRangeAsync(startDate, endDate))
            .ReturnsAsync([]);

        var query = new GetOrdersByDateRangeQuery(startDate, endDate);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Count, Is.EqualTo(0));
            Assert.That(result.Message, Is.EqualTo("Found 0 orders in date range."));
        });

        MockOrderRepository.Verify(repo => repo.GetOrdersByDateRangeAsync(startDate, endDate), Times.Once);
    }
}