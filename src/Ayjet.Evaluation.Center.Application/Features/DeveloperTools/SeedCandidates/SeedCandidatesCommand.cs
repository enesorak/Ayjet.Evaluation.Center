// SeedCandidatesCommand.cs
using MediatR;
namespace Ayjet.Evaluation.Center.Application.Features.DeveloperTools.SeedCandidates;
public record SeedCandidatesCommand(int Count) : IRequest<int>;