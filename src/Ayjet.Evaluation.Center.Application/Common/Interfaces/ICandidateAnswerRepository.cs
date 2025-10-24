using Ayjet.Evaluation.Center.Domain.Entities;

namespace Ayjet.Evaluation.Center.Application.Common.Interfaces;

public interface ICandidateAnswerRepository : IRepository<CandidateAnswer>
{
    // === YENİ METODU EKLEYİN ===
    Task<CandidateAnswer?> GetByAssignmentAndQuestionAsync(string assignmentId, int questionId, CancellationToken cancellationToken = default);
}