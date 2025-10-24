using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Models;

namespace Ayjet.Evaluation.Center.Application.Common.Interfaces;

public interface ITestDefinitionRepository : IRepository<TestDefinition>
{
    Task<DashboardStats> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
    
    IQueryable<TestDefinition> GetAllWithDetails(); // Yeni metot
    
    Task<TestDefinition?> GetByIdWithQuestionsAsync(string id, CancellationToken cancellationToken = default);



}