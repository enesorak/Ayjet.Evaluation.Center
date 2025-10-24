using AAyjet.Evaluation.Center.Persistence.Context;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AAyjet.Evaluation.Center.Persistence.Repositories;

public class MultipleChoiceQuestionRepository : Repository<MultipleChoiceQuestion>, IMultipleChoiceQuestionRepository
{
    public MultipleChoiceQuestionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<MultipleChoiceQuestion>> GetQuestionsByTestDefinitionIdAsync(string testDefinitionId, CancellationToken cancellationToken = default)
    {
        // Soruları çekerken, ilişkili olduğu çevirileri (Translations) ve seçenekleri (Options),
        // seçeneklerin de çevirilerini (Translations) veritabanından tek seferde getirmek için Include/ThenInclude kullanıyoruz.
        return await _context.MultipleChoiceQuestions
            .Where(q => q.TestDefinitionId == testDefinitionId)
            .Include(q => q.Translations)
            .Include(q => q.Options)
            .ThenInclude(o => o.Translations)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<int> GetMaxDisplayOrderAsync(string testDefinitionId)
    {
        return await _context.MultipleChoiceQuestions
            .Where(q => q.TestDefinitionId == testDefinitionId)
            .MaxAsync(q => (int?)q.DisplayOrder) ?? 0;
    }
    
    
    public async Task<MultipleChoiceQuestion?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.MultipleChoiceQuestions
            .Include(q => q.Translations)
            .Include(q => q.Options).ThenInclude(o => o.Translations)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
    }
    
    public IQueryable<MultipleChoiceQuestion> GetListByTestDefinitionId(string testDefinitionId, string? searchTerm)
    {
        // 1. Start with the base query and initial filter.
        var query = _context.MultipleChoiceQuestions
            .Where(q => q.TestDefinitionId == testDefinitionId && q.IsActive);

        // 2. Conditionally apply the search filter.
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(q => 
                (q.QuestionCode != null && q.QuestionCode.ToLower().Contains(searchTerm.ToLower())) ||
                q.Translations.Any(t => t.Text.ToLower().Contains(searchTerm.ToLower()))
            );
        }

        // 3. Apply all Includes, Ordering, and execute the query at the very end.
        return query.Include(q => q.Translations)
            .Include(q => q.Options).ThenInclude(o => o.Translations);
    }
    
    public async Task<List<MultipleChoiceQuestion>> GetRandomQuestionsAsync(string testDefinitionId, int count, CancellationToken cancellationToken = default)
    {
        return await _context.MultipleChoiceQuestions
            .Where(q => q.TestDefinitionId == testDefinitionId && q.IsActive)
            .OrderBy(q => Guid.NewGuid()) // Bu komut, rastgele sıralamayı veritabanı sunucusunda yapar.
            .Take(count) // Sadece istenen sayıda kaydı alır.
            .ToListAsync(cancellationToken); // Sadece bu az sayıdaki kayıt hafızaya çekilir.
    }
}