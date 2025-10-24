using Ayjet.Evaluation.Center.Domain.Entities;

namespace Ayjet.Evaluation.Center.Application.Common.Interfaces;

public interface IPsychometricQuestionRepository : IRepository<PsychometricQuestion>
{
     Task<PsychometricQuestion?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    
    Task<ILookup<int, QuestionScaleMapping>> GetMappingsForQuestionsAsync(IEnumerable<int> questionIds);

    IQueryable<PsychometricQuestion> GetListByTestDefinitionId(string testDefinitionId, string? searchTerm);
    Task<List<PsychometricQuestion>> GetAllByTestDefinitionIdAsync(string testDefinitionId); // <-- YENİ METOT

}