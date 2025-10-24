namespace Ayjet.Evaluation.Center.Domain.Common;


// Sadece ID'si string (GUID) olan varlıklar için bir temel sınıf.
public abstract class BaseEntityWithGuid : BaseEntity<string>
{
    // Bu sınıftan türeyen her nesne oluşturulduğunda,
    // ID'si otomatik olarak bir GUID ile doldurulur.
    protected BaseEntityWithGuid()
    {
        Id = Guid.NewGuid().ToString();
    }
}