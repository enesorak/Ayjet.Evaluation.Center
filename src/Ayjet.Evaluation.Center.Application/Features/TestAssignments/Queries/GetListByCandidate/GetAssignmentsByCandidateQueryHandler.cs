using AutoMapper;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Application.Features.TestAssignments.Queries.GetList;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Queries.GetListByCandidate;

public class GetAssignmentsByCandidateQueryHandler : IRequestHandler<GetAssignmentsByCandidateQuery, List<TestAssignmentDto>>
{
    private readonly ITestAssignmentRepository _assignmentRepo;
    private readonly IMapper _mapper;
    public GetAssignmentsByCandidateQueryHandler(ITestAssignmentRepository repo, IMapper mapper) 
        => (_assignmentRepo, _mapper) = (repo, mapper);

    public async Task<List<TestAssignmentDto>> Handle(GetAssignmentsByCandidateQuery request, CancellationToken ct)
    {
        var assignments = await _assignmentRepo.GetByCandidateIdAsync(request.CandidateId, ct);
        return _mapper.Map<List<TestAssignmentDto>>(assignments);
    }
}