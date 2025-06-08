using Market.Application.Features.Orders.Queries.GetOrderByOrderNumber;
using Market.Domain.Entities.Market;
using Moq;

namespace Market.ApplicationTest.Orders.Queries;

[TestFixture]
public class GetOrderByNumberHandlerTests : TestBase
{
    private GetOrderByNumberHandler _handler = null!;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        _handler = new GetOrderByNumberHandler(MockUnitOfWork.Object, Mapper);
    }

    [Test]
    public async Task Handle_ShouldReturnSuccessResponse_WhenOrderExists()
    {
        // Arrange
        var order = new Order
        {
            Id = 1,
            OrderNumber = "ORD001",
            OrderDetails = new List<OrderDetail>
            {
                new OrderDetail { Id = 1, Product = new Product{Name = "Test"}, Quantity = 2 }
            }
        };

        MockOrderRepository
            .Setup(repo => repo.GetByOrderNumberAsync(order.OrderNumber))
            .ReturnsAsync(order);

        var query = new GetOrderByNumberQuery("ORD001");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.OrderNumber, Is.EqualTo("ORD001"));
            Assert.That(result.Data.OrderDetails, Is.Not.Empty);
        });

        MockOrderRepository.Verify(repo => repo.GetByOrderNumberAsync(order.OrderNumber), Times.Once);
    }
}