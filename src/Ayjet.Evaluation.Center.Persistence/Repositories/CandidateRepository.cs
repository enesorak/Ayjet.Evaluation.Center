using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Enums;
using Ayjet.Evaluation.Center.Domain.Models;
using Ayjet.Evaluation.Center.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Ayjet.Evaluation.Center.Persistence.Repositories;

public class CandidateRepository : Repository<Candidate>, ICandidateRepository
{
    public CandidateRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Candidate?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Candidates.FirstOrDefaultAsync(c => c.Email == email, cancellationToken);
    }
    
    public IQueryable<Candidate> GetAsQueryable()
    {
        return _context.Candidates.AsQueryable();
    }
    
    
    public async Task<bool> IsInitialCodeUniqueAsync(string initialCode)
    {
        return !await _context.Candidates.AnyAsync(c => c.InitialCode == initialCode);
    }
    
    public async Task<CandidateCounts> GetCandidateCountsAsync(CancellationToken cancellationToken = default)
    {
        var activeCount = await _context.Candidates.CountAsync(c => !c.IsArchived, cancellationToken);
        var inProgressCount = await _context.Candidates.CountAsync(c => !c.IsArchived && c.TestAssignments.Any(a => a.Status == TestAssignmentStatus.InProgress), cancellationToken);
        var completedCount = await _context.Candidates.CountAsync(c => !c.IsArchived && c.TestAssignments.Any(a => a.Status == TestAssignmentStatus.Completed), cancellationToken);
        var archivedCount = await _context.Candidates.CountAsync(c => c.IsArchived, cancellationToken);

        return new CandidateCounts
        {
            Active = activeCount,
            InProgress = inProgressCount,
            Completed = completedCount,
            Archived = archivedCount
        };
    }
}