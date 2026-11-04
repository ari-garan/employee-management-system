namespace EmployeeManagement.Domain.Common;

/// <summary>
/// Base class for all domain entities providing identity and audit timestamps.
/// Demonstrates encapsulation and inheritance in C#.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAtUtc { get; protected set; }
    public DateTime? UpdatedAtUtc { get; protected set; }

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAtUtc = DateTime.UtcNow;
    }

    protected BaseEntity(Guid id)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public virtual void MarkUpdated()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }
}

