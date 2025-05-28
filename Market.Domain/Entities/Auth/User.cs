using Market.Domain.Entities.Common;
using Market.Domain.Entities.Market;

namespace Market.Domain.Entities.Auth;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public long RoleId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Role? Role { get; set; }
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = [];

    #region Created/Updated Entities
    public virtual ICollection<Location> CreatedLocations { get; set; } = [];
    public virtual ICollection<Location> UpdatedLocations { get; set; } = [];
    public virtual ICollection<Vendor> CreatedVendors { get; set; } = [];
    public virtual ICollection<Vendor> UpdatedVendors { get; set; } = [];
    public virtual ICollection<Category> CreatedCategories { get; set; } = [];
    public virtual ICollection<Category> UpdatedCategories { get; set; } = [];
    public virtual ICollection<VendorLocation> CreatedVendorLocations { get; set; } = [];
    public virtual ICollection<VendorLocation> UpdatedVendorLocations { get; set; } = [];
    public virtual ICollection<Product> CreatedProducts { get; set; } = [];
    public virtual ICollection<Product> UpdatedProducts { get; set; } = [];
    public virtual ICollection<Discount> CreatedDiscounts { get; set; } = [];
    public virtual ICollection<Discount> UpdatedDiscounts { get; set; } = [];
    public virtual ICollection<Order> Orders { get; set; } = [];
    public virtual ICollection<Order> CreatedOrders { get; set; } = [];
    public virtual ICollection<Order> UpdatedOrders { get; set; } = [];
    public virtual ICollection<Procurement> CreatedProcurements { get; set; } = [];
    public virtual ICollection<Procurement> UpdatedProcurements { get; set; } = [];
    public virtual ICollection<Payment> CreatedPayments { get; set; } = [];
    public virtual ICollection<Payment> UpdatedPayments { get; set; } = [];
    #endregion

    // Computed properties
    public string FullName => $"{FirstName} {LastName}".Trim();
}