using Ayjet.Evaluation.Center.Domain.Entities;

namespace Ayjet.Evaluation.Center.Application.Common.Interfaces;

public interface ITestAssignmentRepository : IRepository<TestAssignment>
{
    /// <summary>
    /// Bir test atamasını, ilişkili olduğu Aday (Candidate) ve Test Tanımı (TestDefinition)
    /// bilgileriyle birlikte getirir. Genellikle adayın sınav başlangıç ekranı için kullanılır.
    /// </summary>
    /// <param name="assignmentId">Test atamasının ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İlişkili varlıklarıyla birlikte TestAssignment nesnesi.</returns>
    Task<TestAssignment?> GetByIdWithDetailsAsync(string assignmentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bir test atamasını, puanlama için gerekli olan tüm detaylarıyla (Adayın Cevapları ve Seçtiği Seçenekler)
    /// birlikte getirir.
    /// </summary>
    /// <param name="assignmentId">Test atamasının ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Puanlama için hazır, tüm detayları içeren TestAssignment nesnesi.</returns>
    Task<TestAssignment?> GetForScoringAsync(string assignmentId, CancellationToken cancellationToken = default);
    
    
    
    Task<TestAssignment?> GetProgressAsync(string assignmentId, CancellationToken cancellationToken = default);

    Task<List<TestAssignment>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);
    
    IQueryable<TestAssignment> GetAllWithDetails();
    
    Task<List<TestAssignment>> GetByCandidateIdAsync(string candidateId, CancellationToken cancellationToken = default);
    Task<TestAssignment?> GetByIdForUpdateAsync(string id, CancellationToken cancellationToken = default);
}