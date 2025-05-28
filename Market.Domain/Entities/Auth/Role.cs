using Market.Domain.Entities.Common;

namespace Market.Domain.Entities.Auth;

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<User> Users { get; set; } = [];
}