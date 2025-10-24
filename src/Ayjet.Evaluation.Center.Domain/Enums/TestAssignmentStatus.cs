namespace Ayjet.Evaluation.Center.Domain.Enums;

public enum TestAssignmentStatus
{
    Pending = 1, // Atandı, henüz başlanmadı
    InProgress = 2, // Aday tarafından başlatıldı
    Completed = 3, // Tamamlandı
    Expired = 4 // Süresi geçti
}