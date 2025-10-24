using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Commands.Create;

// Artık e-posta listesi yerine Aday ID'leri listesi alıyor
public record AssignTestCommand(
    string TestDefinitionId,
    List<string> CandidateIds, // <-- Değişti
    int? TimeLimitInMinutes,
    int? QuestionCount,
    string Language,
    int DaysToExpire = 3
) : IRequest<List<string>>;