using AAyjet.Evaluation.Center.Persistence.Context;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Enums;
using Ayjet.Evaluation.Center.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AAyjet.Evaluation.Center.Persistence.Repositories;

public class TestDefinitionRepository : Repository<TestDefinition>, ITestDefinitionRepository
{
    // Constructor, base class'ın constructor'ını çağırıyor.
    public TestDefinitionRepository(ApplicationDbContext context) : base(context)
    {
    }
    
    public async Task<DashboardStats> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        var totalTests = await _context.TestDefinitions.CountAsync(cancellationToken);
        var pendingAssignments = await _context.TestAssignments.CountAsync(a => a.Status == TestAssignmentStatus.Pending, cancellationToken);
        var completedToday = await _context.TestAssignments.CountAsync(a => a.CompletedAt.HasValue && a.CompletedAt.Value.Date == DateTime.UtcNow.Date, cancellationToken);

        return new DashboardStats
        {
            TotalTestDefinitions = totalTests,
            PendingAssignments = pendingAssignments,
            CompletedToday = completedToday
        };
    }
    
    
    public IQueryable<TestDefinition> GetAllWithDetails()
    {
        // Şimdilik ek bir Include yok, ama ileride eklenebilir.
        return _context.TestDefinitions.AsNoTracking().OrderBy(t => t.Title);
    }
    
    
    
    public async Task<TestDefinition?> GetByIdWithQuestionsAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _context.TestDefinitions
            .Include(td => td.MultipleChoiceQuestions) // Include Multiple Choice questions
            .Include(td => td.PsychometricQuestions)  // ALSO Include Psychometric questions
            .FirstOrDefaultAsync(td => td.Id == id, cancellationToken);

    }
    
    
}