using Ayjet.Evaluation.Center.Application.Features.Candidates.Queries.GetList;
using Ayjet.Evaluation.Center.Domain.Enums;

namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Queries.GetById;

public class TestAssignmentDetailDto
{
    public string Id { get; set; } = string.Empty;
    public TestType TestType { get; set; }
    public CandidateDto Candidate { get; set; } = null!;
}