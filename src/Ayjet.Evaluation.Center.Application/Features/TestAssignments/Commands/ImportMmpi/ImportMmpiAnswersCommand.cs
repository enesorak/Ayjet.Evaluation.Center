using MediatR;
using Microsoft.AspNetCore.Http; // IFormFile için

namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Commands.ImportMmpi;

// Artık DTO yerine IFormFile alıyor
public record ImportMmpiAnswersCommand(
    string CandidateId, 
    IFormFile AnswerFile, // <-- Yüklenen CSV dosyası
    DateTime? CompletedAtOverride // Opsiyonel tamamlama tarihi
    ) : IRequest<string>; // Yine yeni Assignment ID'sini dönecek