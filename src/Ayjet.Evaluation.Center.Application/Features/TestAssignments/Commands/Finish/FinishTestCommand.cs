using MediatR;
using System.Collections.Generic;

// Namespace'in projenle uyumlu olduğundan emin ol
namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Commands.Finish;

/// <summary>
/// Bir sınavı tamamlamak ve adayın cevaplarını göndermek için kullanılan komut.
/// </summary>
public class FinishTestCommand : IRequest
{
    /// <summary>
    /// Tamamlanan sınavın benzersiz kimliği (AssignmentId).
    /// </summary>
    public required string AssignmentId { get; set; }

    /// <summary>
    /// Adayın cevapları.
    /// Key: Soru ID'si (QuestionId).
    /// Value: Adayın cevabı (Psikometrik için 0, 1, 2; Çoktan seçmeli için Seçenek ID'si).
    /// </summary>
    public required Dictionary<string, int?> Answers { get; set; }
}