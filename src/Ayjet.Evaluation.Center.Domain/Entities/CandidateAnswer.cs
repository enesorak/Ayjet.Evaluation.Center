using Ayjet.Evaluation.Center.Domain.Common;

namespace Ayjet.Evaluation.Center.Domain.Entities;

// Bu varlığın ID'si için int kullanıyoruz, çünkü potansiyel olarak
// milyonlarca cevap kaydı olabilir ve int daha performanslıdır.
public class CandidateAnswer : BaseEntity<int>
{
    public string TestAssignmentId { get; set; }
    public TestAssignment TestAssignment { get; set; }
    public int? SelectedOptionId { get; set; } // <-- BU SATIRI EKLE

    public string QuestionSnapshotJson { get; set; } = string.Empty;

    // Puanlama ve raporlama için bu alana ihtiyacımız var.
    public bool IsCorrectAtTimeOfAnswer { get; set; }

    // Geçmişe yönelik bağlantı (opsiyonel)
    public int? MultipleChoiceQuestionId { get; set; }
    public MultipleChoiceQuestion? MultipleChoiceQuestion { get; set; }

    public int? PsychometricResponse { get; set; } // <-- Ekle (0:No Idea, 1:True, 2:False)
    
    
    // === YENİ EKLENEN İLİŞKİ ===
    public int? PsychometricQuestionId { get; set; }
    public PsychometricQuestion? PsychometricQuestion { get; set; }

    public DateTime AnsweredAt { get; set; }
}