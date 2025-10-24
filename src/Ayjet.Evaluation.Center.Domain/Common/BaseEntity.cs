namespace Ayjet.Evaluation.Center.Domain.Common;

public abstract class BaseEntity<TId>
{
    public TId Id { get; protected set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Bu sınıftan türeyen her nesne oluşturulduğunda,
    // ID'si otomatik olarak bir GUID ile doldurulur.
    
    protected BaseEntity()
    {
        CreatedAt = DateTime.UtcNow;
    }
}