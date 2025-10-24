using Ayjet.Evaluation.Center.Domain.Entities;

namespace Ayjet.Evaluation.Center.Application.Common.Interfaces;

public interface ITestResultRepository : IRepository<TestResult>
{
    Task<TestResult?> GetByAssignmentIdAsync(string assignmentId, CancellationToken cancellationToken = default);
    
    Task<TestResult?> GetByAssignmentIdWithDetailsAsync(string assignmentId, CancellationToken cancellationToken = default);
    
 


}