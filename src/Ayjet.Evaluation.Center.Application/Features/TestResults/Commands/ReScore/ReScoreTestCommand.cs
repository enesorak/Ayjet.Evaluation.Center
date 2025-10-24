using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestResults.Commands.ReScore;

public record ReScoreTestCommand(string AssignmentId) : IRequest;
