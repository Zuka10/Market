using Market.Domain.Entities.Common;

namespace Market.Domain.Entities.Market;

public class Category : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation properties
    public virtual ICollection<Product> Products { get; set; } = [];

    // Computed properties
    public int ProductCount => Products?.Count ?? 0;
    public int AvailableProductCount => Products?.Count(p => p.IsAvailable) ?? 0;
}