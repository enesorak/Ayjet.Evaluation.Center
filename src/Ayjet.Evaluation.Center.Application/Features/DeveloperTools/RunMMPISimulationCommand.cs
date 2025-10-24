using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.DeveloperTools;
public record RunMMPISimulationCommand() : IRequest<Dictionary<int, int>>;
