using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Ayjet.Evaluation.Center.Persistence.Repositories;

public class TestAssignmentRepository : Repository<TestAssignment>, ITestAssignmentRepository
{
    public TestAssignmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<TestAssignment?> GetByIdWithDetailsAsync(string assignmentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.TestAssignments
            .Include(a => a.Candidate) // İlişkili Aday bilgisini de getir
            .Include(a => a.TestDefinition) // İlişkili Test Tanımı bilgisini de getir
            .FirstOrDefaultAsync(a => a.Id == assignmentId, cancellationToken);
    }


    public async Task<TestAssignment?> GetForScoringAsync(string assignmentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.TestAssignments
            .Include(a => a.Candidate) // <-- EKSİK OLAN VE HATAYI ÇÖZEN SATIR
            .Include(a => a.TestDefinition)
            .Include(a => a.CandidateAnswers)
            .Include(a => a.AssignedQuestions)
            .ThenInclude(aq => aq.PsychometricQuestion!)
            .ThenInclude(pq => pq.ScaleMappings)
            .ThenInclude(sm => sm.PsychometricScale)
            .AsNoTracking() // Puanlama sadece okuma yaptığı için bu optimizasyon kalabilir
            .FirstOrDefaultAsync(a => a.Id == assignmentId, cancellationToken);    }

    public async Task<TestAssignment?> GetProgressAsync(string assignmentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.TestAssignments
            .Include(a => a.Candidate)
            .Include(a => a.TestDefinition)
            .Include(a => a.CandidateAnswers)
            
            .Include(a => a.AssignedQuestions)
                .ThenInclude(aq => aq.MultipleChoiceQuestion!)
                    .ThenInclude(mcq => mcq.Translations)
           
            .Include(a => a.AssignedQuestions)
                .ThenInclude(aq => aq.MultipleChoiceQuestion!)
                    .ThenInclude(mcq => mcq.Options)
                        .ThenInclude(o => o.Translations)
            
            .Include(a => a.AssignedQuestions)
                .ThenInclude(aq => aq.PsychometricQuestion!)
                    .ThenInclude(pq => pq.Translations)
            
            .Include(a => a.AssignedQuestions)
            // For each assigned question, also include the actual question details
                .ThenInclude(aq => aq.PsychometricQuestion)
            // For each question, also include its scoring rules (mappings)
                 .ThenInclude(pq => pq!.ScaleMappings)
            // For each rule, also include the scale name (L, F, K...)
                     .ThenInclude(sm => sm.PsychometricScale)
           
            
            
            .AsSplitQuery() // <-- BU SATIRI EKLEYİN
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == assignmentId, cancellationToken);
    }



    public async Task<List<TestAssignment>> GetAllWithDetailsAsync(CancellationToken ct)
    {
        return await _context.TestAssignments
            .Include(a => a.Candidate)
            .Include(a => a.TestDefinition)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
    }

    public IQueryable<TestAssignment> GetAllWithDetails()
    {
        return _context.TestAssignments
            .Include(a => a.Candidate)
            .Include(a => a.TestDefinition)
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt);
    }

    public async Task<List<TestAssignment>> GetByCandidateIdAsync(string candidateId,
        CancellationToken cancellationToken = default)
    {
        return await _context.TestAssignments
            .Where(a => a.CandidateId == candidateId)
            .Include(a => a.TestDefinition) // Test başlığını almak için
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }
    
    
    // TestAssignmentRepository.cs içine ekleyin:
    public async Task<TestAssignment?> GetByIdForUpdateAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.TestAssignments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }
}