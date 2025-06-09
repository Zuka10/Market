using Market.Application.DTOs.Market;
using Market.Application.Features.Orders.Commands.CreateOrder;
using Market.Domain.Entities.Market;
using Market.Domain.Enums;
using Moq;

namespace Market.ApplicationTest.Orders.Commands;

[TestFixture]
public class CreateOrderHandlerTests : TestBase
{
    private CreateOrderHandler _handler = null!;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        _handler = new CreateOrderHandler(MockUnitOfWork.Object, Mapper);
    }

    [Test]
    public async Task Handle_ShouldCreateOrderAndDetailsSuccessfully()
    {
        // Arrange
        var command = new CreateOrderCommand(
            OrderNumber: "ORD-1001",
            OrderDate: DateTime.UtcNow,
            Total: 1000,
            SubTotal: 900,
            TotalCommission: 100,
            Status: OrderStatus.Completed,
            LocationId: 1,
            DiscountId: 2,
            DiscountAmount: 50,
            UserId: 1,
            CustomerName: "John Doe",
            CustomerPhone: "1234567890",
            Notes: "Urgent delivery",
            OrderDetails: new List<OrderDetailDto>
            {
                new() { ProductId = 1, Quantity = 2, UnitPrice = 200, LineTotal = 400, CostPrice = 150, Profit = 100 },
                new() { ProductId = 2, Quantity = 3, UnitPrice = 200, LineTotal = 600, CostPrice = 500, Profit = 100 }
            });

        var createdOrder = new Order
        {
            Id = 42,
            OrderNumber = command.OrderNumber,
            Status = command.Status,
            OrderDetails = command.OrderDetails.Select(od => new OrderDetail
            {
                ProductId = od.ProductId,
                Quantity = od.Quantity,
                UnitPrice = od.UnitPrice,
                LineTotal = od.LineTotal,
                CostPrice = od.CostPrice,
                Profit = od.Profit
            }).ToList(),
        };

        MockOrderRepository
            .Setup(repo => repo.AddAsync(It.IsAny<Order>()))
            .ReturnsAsync(createdOrder);

        MockOrderDetailRepository
            .Setup(repo => repo.AddAsync(It.IsAny<OrderDetail>()))
            .ReturnsAsync((OrderDetail od) => od);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(42));
            Assert.That(result.Message, Is.EqualTo("Order created successfully."));
        });

        MockOrderRepository.Verify(repo => repo.AddAsync(It.IsAny<Order>()), Times.Once);
        MockOrderDetailRepository.Verify(repo => repo.AddAsync(It.IsAny<OrderDetail>()), Times.Exactly(command.OrderDetails.Count));
    }
}