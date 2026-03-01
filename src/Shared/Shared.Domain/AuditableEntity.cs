namespace Shared.Domain;

public interface IAuditableEntity
{
    Guid CreatedBy { get; }
    DateTime CreatedAt { get; }
    Guid? ModifiedBy { get; }
    DateTime? ModifiedAt { get; }
}

public abstract class AuditableEntity : Entity, IAuditableEntity
{
    public Guid CreatedBy { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public Guid? ModifiedBy { get; protected set; }
    public DateTime? ModifiedAt { get; protected set; }
}

