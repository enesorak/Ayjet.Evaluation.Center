using Ayjet.Evaluation.Center.Domain.Entities;

namespace Ayjet.Evaluation.Center.Application.Common.Interfaces;

public interface IMultipleChoiceQuestionRepository : IRepository<MultipleChoiceQuestion>
{
    Task<List<MultipleChoiceQuestion>> GetQuestionsByTestDefinitionIdAsync(string testDefinitionId, CancellationToken cancellationToken = default);
    
    Task<int> GetMaxDisplayOrderAsync(string testDefinitionId);
    
    
    Task<MultipleChoiceQuestion?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
   

    IQueryable<MultipleChoiceQuestion> GetListByTestDefinitionId(string testDefinitionId, string? searchTerm);

    Task<List<MultipleChoiceQuestion>> GetRandomQuestionsAsync(string testDefinitionId, int count, CancellationToken cancellationToken = default);

}
