using Market.Application.Features.Orders.Queries.GetOrderByUser;
using Market.Domain.Entities.Market;
using Moq;

namespace Market.ApplicationTest.Orders.Queries;

[TestFixture]
public class GetOrdersByUserHandlerTests : TestBase
{
    private GetOrdersByUserHandler _handler = null!;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        _handler = new GetOrdersByUserHandler(MockUnitOfWork.Object, Mapper);
    }

    [Test]
    public async Task Handle_ShouldReturnSuccessResponse_WhenOrdersExistForUser()
    {
        // Arrange
        var userId = 1;
        var testOrders = new List<Order>
        {
            CreateTestOrderWithDetails(1),
            CreateTestOrderWithDetails(2)
        };

        MockOrderRepository
            .Setup(repo => repo.GetOrdersByUserAsync(userId))
            .ReturnsAsync(testOrders);

        var query = new GetOrdersByUserQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Count, Is.EqualTo(testOrders.Count));
            Assert.That(result.Message, Is.EqualTo($"Found {testOrders.Count} orders for user."));
        });

        MockOrderRepository.Verify(repo => repo.GetOrdersByUserAsync(userId), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldReturnSuccessResponse_WithEmptyList_WhenNoOrdersExistForUser()
    {
        // Arrange
        var userId = 1;

        MockOrderRepository
            .Setup(repo => repo.GetOrdersByUserAsync(userId))
            .ReturnsAsync([]);

        var query = new GetOrdersByUserQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!, Is.Empty);
            Assert.That(result.Message, Is.EqualTo("Found 0 orders for user."));
        });

        MockOrderRepository.Verify(repo => repo.GetOrdersByUserAsync(userId), Times.Once);
    }
}