using Market.Domain.Entities.Auth;

namespace Market.Domain.Entities.Common;

public class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public long CreatedBy { get; set; }
    public long UpdatedBy { get; set; }

    public virtual User? Creator { get; set; }
    public virtual User? Updater { get; set; }
}