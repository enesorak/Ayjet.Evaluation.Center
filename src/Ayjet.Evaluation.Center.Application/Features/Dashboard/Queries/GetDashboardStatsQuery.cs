using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.Dashboard.Queries;

public record GetDashboardStatsQuery() : IRequest<DashboardStatsDto>;