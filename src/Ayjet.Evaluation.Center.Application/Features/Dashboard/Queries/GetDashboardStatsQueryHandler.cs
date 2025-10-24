using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.Dashboard.Queries;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly ITestDefinitionRepository _testDefinitionRepository;

    public GetDashboardStatsQueryHandler(ITestDefinitionRepository testDefinitionRepository)
    {
        _testDefinitionRepository = testDefinitionRepository;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var stats = await _testDefinitionRepository.GetDashboardStatsAsync(cancellationToken);

        // Domain modelini, Application DTO'suna çeviriyoruz.
        return new DashboardStatsDto(
            stats.TotalTestDefinitions,
            stats.PendingAssignments,
            stats.CompletedToday
        );
    }
}