using AutoMapper;
using Market.Application.MappingProfiles;
using Market.Domain.Abstractions;
using Market.Domain.Abstractions.Repositories.Market;
using Market.Domain.Entities.Auth;
using Market.Domain.Entities.Market;
using Market.Domain.Enums;
using Market.Domain.Filters;
using Moq;

namespace Market.ApplicationTest;

[TestFixture]
public abstract class TestBase
{
    protected Mock<IUnitOfWork> MockUnitOfWork { get; private set; } = null!;
    protected Mock<IOrderRepository> MockOrderRepository { get; private set; } = null!;
    protected Mock<IOrderDetailRepository> MockOrderDetailRepository { get; private set; } = null!;
    protected IMapper Mapper { get; private set; } = null!;

    private static int _idCounter = 1;

    [SetUp]
    public virtual void SetUp()
    {
        // Reset ID counter for each test
        _idCounter = 1;

        // Initialize mocks
        MockUnitOfWork = new Mock<IUnitOfWork>();
        MockOrderRepository = new Mock<IOrderRepository>();
        MockOrderDetailRepository = new Mock<IOrderDetailRepository>();

        // Setup UnitOfWork to return mocked repositories
        MockUnitOfWork.Setup(x => x.Orders).Returns(MockOrderRepository.Object);
        MockUnitOfWork.Setup(x => x.OrderDetails).Returns(MockOrderDetailRepository.Object);

        // Initialize AutoMapper
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<OrderMappingProfile>();
            cfg.AddProfile<ProductMappingProfile>();
            cfg.AddProfile<OrderDetailMappingProfile>();
        });
        Mapper = configuration.CreateMapper();
    }

    [TearDown]
    public virtual void TearDown()
    {
        // Clean up mocks
        MockUnitOfWork.Reset();
        MockOrderRepository.Reset();
        MockOrderDetailRepository.Reset();
    }

    // Helper methods for creating test objects
    protected static long GetNextId() => _idCounter++;

    protected static User CreateTestUser(long? id = null, string? username = null, string? email = null)
    {
        var userId = id ?? GetNextId();
        return new User
        {
            Id = userId,
            Username = username ?? $"testuser{userId}",
            Email = email ?? $"test{userId}@example.com",
            FirstName = $"First{userId}",
            LastName = $"Last{userId}",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    protected static Location CreateTestLocation(long? id = null, string? name = null, string? city = null)
    {
        var locationId = id ?? GetNextId();
        return new Location
        {
            Id = locationId,
            Name = name ?? $"Test Location {locationId}",
            City = city ?? $"Test City {locationId}",
            Address = $"Test Address {locationId}",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    protected static Product CreateTestProduct(long? id = null, string? name = null, decimal? price = null)
    {
        var productId = id ?? GetNextId();
        return new Product
        {
            Id = productId,
            Name = name ?? $"Test Product {productId}",
            Description = $"Test Description {productId}",
            Price = price ?? (productId * 10.50m),
            Unit = "pcs",
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    protected static Order CreateTestOrder(
        long? id = null,
        string? orderNumber = null,
        OrderStatus? status = null,
        decimal? total = null,
        long? userId = null,
        long? locationId = null)
    {
        var orderId = id ?? GetNextId();
        return new Order
        {
            Id = orderId,
            OrderNumber = orderNumber ?? $"ORD-{orderId:D6}",
            OrderDate = DateTime.UtcNow,
            Total = total ?? (orderId * 100m),
            SubTotal = total ?? (orderId * 100m),
            TotalCommission = 0,
            Status = status ?? OrderStatus.Pending,
            LocationId = locationId ?? 1L,
            UserId = userId ?? 1L,
            CustomerName = $"Customer {orderId}",
            CustomerPhone = $"+1-555-{orderId:D4}",
            DiscountAmount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            OrderDetails = new List<OrderDetail>()
        };
    }

    protected static OrderDetail CreateTestOrderDetail(
        long? id = null,
        long? orderId = null,
        long? productId = null,
        int? quantity = null,
        decimal? unitPrice = null)
    {
        var detailId = id ?? GetNextId();
        var qty = quantity ?? 1;
        var price = unitPrice ?? (detailId * 10m);

        return new OrderDetail
        {
            Id = detailId,
            OrderId = orderId ?? 1L,
            ProductId = productId ?? 1L,
            Quantity = qty,
            UnitPrice = price,
            LineTotal = qty * price,
            CostPrice = price * 0.6m,
            Profit = price * 0.4m * qty
        };
    }

    protected static List<Order> CreateTestOrdersForUser(long userId, int count)
    {
        var orders = new List<Order>();
        for (int i = 0; i < count; i++)
        {
            orders.Add(CreateTestOrder(
                id: GetNextId(),
                orderNumber: $"ORD-{GetNextId():D6}",
                userId: userId,
                total: (i + 1) * 50m
            ));
        }
        return orders;
    }

    protected static List<Order> CreateTestOrdersForLocation(long locationId, int count)
    {
        var orders = new List<Order>();
        for (int i = 0; i < count; i++)
        {
            orders.Add(CreateTestOrder(
                id: GetNextId(),
                orderNumber: $"ORD-{GetNextId():D6}",
                locationId: locationId,
                total: (i + 1) * 75m
            ));
        }
        return orders;
    }

    protected static List<Order> CreateTestOrdersWithStatus(OrderStatus status, int count)
    {
        var orders = new List<Order>();
        for (int i = 0; i < count; i++)
        {
            orders.Add(CreateTestOrder(
                id: GetNextId(),
                orderNumber: $"ORD-{GetNextId():D6}",
                status: status,
                total: (i + 1) * 25m
            ));
        }
        return orders;
    }

    protected static List<Order> CreateTestOrdersInDateRange(DateTime startDate, DateTime endDate, int count)
    {
        var orders = new List<Order>();
        var random = new Random(42); // Fixed seed for consistent tests

        for (int i = 0; i < count; i++)
        {
            var randomDays = random.Next((endDate - startDate).Days + 1);
            var orderDate = startDate.AddDays(randomDays);

            var order = CreateTestOrder(
                id: GetNextId(),
                orderNumber: $"ORD-{i + 1:D3}",
                total: (i + 1) * 30m
            );
            order.OrderDate = orderDate;
            orders.Add(order);
        }

        return orders;
    }

    protected static Order CreateTestOrderWithDetails(long orderId)
    {
        var user = CreateTestUser(1L, "testuser", "test@example.com");
        var location = CreateTestLocation(1L, "Main Warehouse");
        var product = CreateTestProduct(1L, "Test Product", 10.50m);

        var orderDetail = CreateTestOrderDetail(
            id: 1L,
            orderId: orderId,
            productId: 1L,
            quantity: 2,
            unitPrice: 10.50m
        );
        orderDetail.Product = product;

        var order = CreateTestOrder(
            id: orderId,
            orderNumber: "ORD-001",
            total: 21.00m,
            userId: 1L,
            locationId: 1L
        );

        order.User = user;
        order.Location = location;
        order.OrderDetails = new List<OrderDetail> { orderDetail };

        return order;
    }

    protected static PagedResult<Order> CreatePagedResult(int itemCount, int page, int pageSize, int totalCount)
    {
        var orders = new List<Order>();
        for (int i = 0; i < itemCount; i++)
        {
            orders.Add(CreateTestOrder(
                id: GetNextId(),
                orderNumber: $"ORD-{GetNextId():D6}",
                total: (i + 1) * 15m
            ));
        }

        var totalPages = totalCount > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 0;
        var hasNextPage = page < totalPages;
        var hasPreviousPage = page > 1;

        return new PagedResult<Order>
        {
            Items = orders,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            HasNextPage = hasNextPage,
            HasPreviousPage = hasPreviousPage
        };
    }
}