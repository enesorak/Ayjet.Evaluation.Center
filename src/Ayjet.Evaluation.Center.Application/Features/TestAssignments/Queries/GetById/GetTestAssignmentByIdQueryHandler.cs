using AutoMapper;
using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Application.Features.Candidates.Queries.GetList;
using Ayjet.Evaluation.Center.Domain.Entities;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Queries.GetById;

public class GetTestAssignmentByIdQueryHandler : IRequestHandler<GetTestAssignmentByIdQuery, TestAssignmentDetailDto>
{
    private readonly ITestAssignmentRepository _assignmentRepo;
    private readonly IMapper _mapper;

    public GetTestAssignmentByIdQueryHandler(ITestAssignmentRepository assignmentRepo, IMapper mapper)
    {
        _assignmentRepo = assignmentRepo;
        _mapper = mapper;
    }

    public async Task<TestAssignmentDetailDto> Handle(GetTestAssignmentByIdQuery request, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepo.GetByIdWithDetailsAsync(request.Id, cancellationToken)
                         ?? throw new NotFoundException(nameof(TestAssignment), request.Id);

        // Manually map to our new DTO
        return new TestAssignmentDetailDto
        {
            Id = assignment.Id,
            TestType = assignment.TestDefinition.Type,
            Candidate = _mapper.Map<CandidateDto>(assignment.Candidate)
        };
    }
}