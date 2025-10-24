using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Models;

namespace Ayjet.Evaluation.Center.Application.Common.Interfaces;

public interface ICandidateRepository : IRepository<Candidate>
{
    Task<Candidate?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    
    IQueryable<Candidate> GetAsQueryable(); // Yeni metot
    
    Task<bool> IsInitialCodeUniqueAsync(string initialCode);
    
    Task<CandidateCounts> GetCandidateCountsAsync(CancellationToken cancellationToken = default); // <-- YENİ METOT



}