using AAyjet.Evaluation.Center.Persistence.Context;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AAyjet.Evaluation.Center.Persistence.Repositories;

public class TestResultRepository : Repository<TestResult>, ITestResultRepository
{
    public TestResultRepository(ApplicationDbContext context) : base(context) { }

    public async Task<TestResult?> GetByAssignmentIdAsync(string assignmentId, CancellationToken cancellationToken = default)
    {
        return await _context.TestResults
            .Include(r => r.TestAssignment)
            .ThenInclude(a => a.Candidate)
            .Include(r => r.TestAssignment)
            .ThenInclude(a => a.TestDefinition)
            .FirstOrDefaultAsync(r => r.TestAssignmentId == assignmentId, cancellationToken);
    }
    
    
    public async Task<TestResult?> GetByAssignmentIdWithDetailsAsync(string assignmentId, CancellationToken cancellationToken = default)
    {
        return await _context.TestResults
            .Include(r => r.TestAssignment)
            .ThenInclude(a => a.Candidate)
            .Include(r => r.TestAssignment)
            .ThenInclude(a => a.TestDefinition)
            .ThenInclude(td => td.MultipleChoiceQuestions) // We still need the original questions for context
            .Include(r => r.TestAssignment)
            .ThenInclude(a => a.CandidateAnswers) // We need the answers to read the snapshots
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.TestAssignmentId == assignmentId, cancellationToken);
    }
}