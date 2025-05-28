using Market.Domain.Abstractions;
using Market.Domain.Entities.Auth;
using Market.Domain.Entities.Market;
using Microsoft.AspNetCore.Mvc;

namespace Market.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public TestController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("test-connection")]
    public async Task<IActionResult> TestConnection()
    {
        try
        {
            var count = await _unitOfWork.Roles.CountAsync();
            return Ok(new { Message = "Connection successful", RoleCount = count });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("seed-test-data")]
    public async Task<IActionResult> SeedTestData()
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Create roles
            var adminRole = new Role { Name = "Admin" };
            var managerRole = new Role { Name = "Manager" };

            await _unitOfWork.Roles.AddAsync(adminRole);
            await _unitOfWork.Roles.AddAsync(managerRole);

            // Create a test user
            var testUser = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashedpassword",
                FirstName = "Test",
                LastName = "User",
                RoleId = adminRole.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(testUser);

            // Create a test location
            var testLocation = new Location
            {
                Name = "Test Market",
                Address = "123 Test Street",
                City = "Test City",
                Country = "Georgia",
                IsActive = true,
                CreatedBy = testUser.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = testUser.Id,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Locations.AddAsync(testLocation);

            // Create a test category
            var testCategory = new Category
            {
                Name = "Test Category",
                Description = "Test category description",
                CreatedBy = testUser.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = testUser.Id,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Categories.AddAsync(testCategory);

            // Create a test product
            var testProduct = new Product
            {
                Name = "Test Product",
                Description = "Test product description",
                Price = 19.99m,
                InStock = 100,
                Unit = "piece",
                LocationId = testLocation.Id,
                CategoryId = testCategory.Id,
                IsAvailable = true,
                CreatedBy = testUser.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = testUser.Id,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Products.AddAsync(testProduct);

            await _unitOfWork.CommitTransactionAsync();

            return Ok(new { Message = "Test data seeded successfully!" });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return BadRequest(new { Error = ex.Message, StackTrace = ex.StackTrace });
        }
    }

    [HttpGet("test-all-repositories")]
    public async Task<IActionResult> TestAllRepositories()
    {
        var results = new Dictionary<string, object>();

        try
        {
            // Test Roles
            var roles = await _unitOfWork.Roles.GetAllAsync();
            results["Roles"] = new { Count = roles.Count(), Data = roles.Take(2) };

            // Test Users
            var users = await _unitOfWork.Users.GetAllAsync();
            results["Users"] = new { Count = users.Count(), Data = users.Take(2) };

            // Test Locations
            var locations = await _unitOfWork.Locations.GetAllAsync();
            results["Locations"] = new { Count = locations.Count(), Data = locations.Take(2) };

            // Test Categories
            var categories = await _unitOfWork.Categories.GetAllAsync();
            results["Categories"] = new { Count = categories.Count(), Data = categories.Take(2) };

            // Test Products
            var products = await _unitOfWork.Products.GetAllAsync();
            results["Products"] = new { Count = products.Count(), Data = products.Take(2) };

            // Test complex queries
            var activeUsers = await _unitOfWork.Users.GetActiveUsersAsync();
            results["ActiveUsers"] = new { Count = activeUsers.Count() };

            var availableProducts = await _unitOfWork.Products.GetAvailableProductsAsync();
            results["AvailableProducts"] = new { Count = availableProducts.Count() };

            return Ok(new { Message = "All repositories tested successfully!", Results = results });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message, StackTrace = ex.StackTrace });
        }
    }

    [HttpGet("test-advanced-queries")]
    public async Task<IActionResult> TestAdvancedQueries()
    {
        var results = new Dictionary<string, object>();

        try
        {
            // Test user with role
            var users = await _unitOfWork.Users.GetAllAsync();
            if (users.Any())
            {
                var firstUser = users.First();
                var userWithRole = await _unitOfWork.Users.GetUserWithRoleAsync(firstUser.Id);
                results["UserWithRole"] = new { User = userWithRole?.Username, Role = userWithRole?.Role?.Name };
            }

            // Test products with details
            var products = await _unitOfWork.Products.GetAllAsync();
            if (products.Any())
            {
                var firstProduct = products.First();
                var productWithDetails = await _unitOfWork.Products.GetProductWithDetailsAsync(firstProduct.Id);
                results["ProductWithDetails"] = new
                {
                    Product = productWithDetails?.Name,
                    Category = productWithDetails?.Category?.Name,
                    Location = productWithDetails?.Location?.Name
                };
            }

            // Test categories with products
            var categoriesWithProducts = await _unitOfWork.Categories.GetCategoriesWithProductsAsync();
            results["CategoriesWithProducts"] = categoriesWithProducts.Select(c => new
            {
                Category = c.Name,
                ProductCount = c.Products?.Count ?? 0
            });

            // Test pagination
            var (pagedProducts, totalCount) = await _unitOfWork.Products.GetPagedProductsAsync(1, 5);
            results["Pagination"] = new { TotalCount = totalCount, PagedCount = pagedProducts.Count() };

            return Ok(new { Message = "Advanced queries tested successfully!", Results = results });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message, StackTrace = ex.StackTrace });
        }
    }

    [HttpDelete("cleanup-test-data")]
    public async Task<IActionResult> CleanupTestData()
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Delete in reverse order of creation due to foreign keys
            var products = await _unitOfWork.Products.GetAllAsync();
            foreach (var product in products.Where(p => p.Name.Contains("Test")))
            {
                await _unitOfWork.Products.DeleteAsync(product.Id);
            }

            var categories = await _unitOfWork.Categories.GetAllAsync();
            foreach (var category in categories.Where(c => c.Name.Contains("Test")))
            {
                await _unitOfWork.Categories.DeleteAsync(category.Id);
            }

            var locations = await _unitOfWork.Locations.GetAllAsync();
            foreach (var location in locations.Where(l => l.Name.Contains("Test")))
            {
                await _unitOfWork.Locations.DeleteAsync(location.Id);
            }

            var users = await _unitOfWork.Users.GetAllAsync();
            foreach (var user in users.Where(u => u.Username.Contains("test")))
            {
                await _unitOfWork.Users.DeleteAsync(user.Id);
            }

            var roles = await _unitOfWork.Roles.GetAllAsync();
            foreach (var role in roles.Where(r => r.Name == "Manager"))
            {
                await _unitOfWork.Roles.DeleteAsync(role.Id);
            }

            await _unitOfWork.CommitTransactionAsync();

            return Ok(new { Message = "Test data cleaned up successfully!" });
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return BadRequest(new { Error = ex.Message });
        }
    }
}