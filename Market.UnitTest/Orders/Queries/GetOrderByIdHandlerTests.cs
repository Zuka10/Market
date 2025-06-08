using Market.Application.Features.Orders.Queries.GetOrderById;
using Moq;

namespace Market.ApplicationTest.Orders.Queries;

[TestFixture]
public class GetOrderByIdHandlerTests : TestBase
{
    private GetOrderByIdHandler _handler = null!;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        _handler = new GetOrderByIdHandler(MockUnitOfWork.Object, Mapper);
    }

    [Test]
    public async Task Handle_ShouldReturnSuccessResponse_WhenOrderExists()
    {
        // Arrange
        var orderId = 1L;
        var testOrder = CreateTestOrderWithDetails(orderId);

        MockOrderRepository
            .Setup(repo => repo.GetOrderWithDetailsAsync(orderId))
            .ReturnsAsync(testOrder);

        var query = new GetOrderByIdQuery(orderId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Message, Is.EqualTo("Order retrieved successfully."));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(orderId));
            Assert.That(result.Data.OrderNumber, Is.EqualTo(testOrder.OrderNumber));
        });

        MockOrderRepository.Verify(repo => repo.GetOrderWithDetailsAsync(1), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldReturnFailureResponse_WhenOrderDoesNotExist()
    {
        // Arrange
        var orderId = 999L;

        MockOrderRepository
            .Setup(repo => repo.GetOrderWithDetailsAsync(orderId))
            .ReturnsAsync((Market.Domain.Entities.Market.Order?)null);

        var query = new GetOrderByIdQuery(orderId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Data, Is.Null);
            Assert.That(result.Errors.ToList(), Does.Contain("Order not found."));
        });
    }
}