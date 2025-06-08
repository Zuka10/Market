using Market.Application.Features.Orders.Queries.GetOrders;
using Market.Domain.Entities.Market;
using Market.Domain.Filters;
using Moq;

namespace Market.ApplicationTest.Orders.Queries;

[TestFixture]
public class GetAllOrdersHandlerTests : TestBase
{
    private GetAllOrdersHandler _handler = null!;

    [SetUp]
    public override void SetUp()
    {
        base.SetUp();
        _handler = new GetAllOrdersHandler(MockUnitOfWork.Object, Mapper);
    }

    [Test]
    public async Task Handle_ShouldReturnPagedOrders_WhenOrdersExist()
    {
        // Arrange
        var orders = new List<Order>
        {
            CreateTestOrderWithDetails(1),
            CreateTestOrderWithDetails(2)
        };

        var pagedOrders = new PagedResult<Order>
        {
            Items = orders,
            TotalCount = 2,
            Page = 1,
            PageSize = 10,
            TotalPages = 1,
            HasNextPage = false,
            HasPreviousPage = false
        };

        var query = new GetOrdersQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        MockOrderRepository
            .Setup(repo => repo.GetOrdersAsync(It.IsAny<OrderFilterParameters>()))
            .ReturnsAsync(pagedOrders);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Items.Count, Is.EqualTo(2));
            Assert.That(result.Data.TotalCount, Is.EqualTo(2));
            Assert.That(result.Data.Page, Is.EqualTo(1));
            Assert.That(result.Message, Is.EqualTo("Retrieved 2 orders successfully."));
        });

        MockOrderRepository.Verify(repo => repo.GetOrdersAsync(It.IsAny<OrderFilterParameters>()), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldReturnEmptyPagedResult_WhenNoOrdersExist()
    {
        // Arrange
        var pagedOrders = new PagedResult<Order>
        {
            Items = [],
            TotalCount = 0,
            Page = 1,
            PageSize = 10,
            TotalPages = 0,
            HasNextPage = false,
            HasPreviousPage = false
        };

        var query = new GetOrdersQuery
        {
            PageNumber = 1,
            PageSize = 10
        };

        MockOrderRepository
            .Setup(repo => repo.GetOrdersAsync(It.IsAny<OrderFilterParameters>()))
            .ReturnsAsync(pagedOrders);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Items.Count, Is.EqualTo(0));
            Assert.That(result.Data.TotalCount, Is.EqualTo(0));
            Assert.That(result.Message, Is.EqualTo("Retrieved 0 orders successfully."));
        });

        MockOrderRepository.Verify(repo => repo.GetOrdersAsync(It.IsAny<OrderFilterParameters>()), Times.Once);
    }
}