using AAyjet.Evaluation.Center.Persistence.Context;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AAyjet.Evaluation.Center.Persistence.Repositories;

public class PsychometricQuestionRepository : Repository<PsychometricQuestion>, IPsychometricQuestionRepository
{
    public PsychometricQuestionRepository(ApplicationDbContext context) : base(context) { }
    
    
    public IQueryable<PsychometricQuestion> GetListByTestDefinitionId(string testDefinitionId, string? searchTerm)
    {
        // 1. Start with the base IQueryable and apply the first filter
        var query = _context.PsychometricQuestions
            .Where(q => q.TestDefinitionId == testDefinitionId && q.IsActive);

        // 2. Conditionally apply the search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(q => 
                q.Translations.Any(t => t.Text.ToLower().Contains(searchTerm.ToLower()))
            );
        }
    
        // 3. Apply Includes, Ordering, and execute the query at the very end
        return query
                .Include(q => q.Translations) // Include is now here
            ;
    }
    
    
    public async Task<PsychometricQuestion?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.PsychometricQuestions
            .Include(q => q.Translations)
            .Include(q => q.ScaleMappings)
            .ThenInclude(sm => sm.PsychometricScale)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
    }
    
    
    public async Task<ILookup<int, QuestionScaleMapping>> GetMappingsForQuestionsAsync(IEnumerable<int> questionIds)
    {
        var mappings = await _context.QuestionScaleMappings
            .Where(m => questionIds.Contains(m.PsychometricQuestionId))
            .Include(m => m.PsychometricScale) // Ölçek adını (L,F,K) almak için
            .ToListAsync();

        return mappings.ToLookup(m => m.PsychometricQuestionId);
    }
    
    public async Task<List<PsychometricQuestion>> GetAllByTestDefinitionIdAsync(string testDefinitionId)
    {
        return await _context.PsychometricQuestions
            .Where(q => q.TestDefinitionId == testDefinitionId)
            .OrderBy(q => q.DisplayOrder)
            .ToListAsync();
    }
}