using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Ayjet.Evaluation.Center.Persistence.Repositories;

public class CandidateAnswerRepository : Repository<CandidateAnswer>, ICandidateAnswerRepository
{
    public CandidateAnswerRepository(ApplicationDbContext context) : base(context) { }
    
    public async Task<CandidateAnswer?> GetByAssignmentAndQuestionAsync(string assignmentId, int questionId, CancellationToken cancellationToken = default)
    {
        return await _context.CandidateAnswers
            .FirstOrDefaultAsync(a => 
                    a.TestAssignmentId == assignmentId && 
                    a.MultipleChoiceQuestionId == questionId, 
                cancellationToken);
    }
    
    
}