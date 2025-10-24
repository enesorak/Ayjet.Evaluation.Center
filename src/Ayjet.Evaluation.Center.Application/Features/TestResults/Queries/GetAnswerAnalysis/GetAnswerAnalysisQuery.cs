using Ayjet.Evaluation.Center.Application.Features.TestResults.Queries.GetByAssignmentId;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestResults.Queries.GetAnswerAnalysis;

public record GetAnswerAnalysisQuery(string AssignmentId) : IRequest<List<AnswerDetailDto>>;
