using AutoMapper;
using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Enums;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Queries.GetTestStartInfo;

// Konum: GetTestStartInfoQueryHandler.cs

public class GetTestStartInfoQueryHandler : IRequestHandler<GetTestStartInfoQuery, TestStartInfoDto>
{
    private readonly ITestAssignmentRepository _assignmentRepository;
    private readonly IMapper _mapper; // DTO'yu artık kullanmıyoruz ama kalsın

    public GetTestStartInfoQueryHandler(ITestAssignmentRepository assignmentRepository, IMapper mapper)
    {
        _assignmentRepository = assignmentRepository;
        _mapper = mapper;
    }

    public async Task<TestStartInfoDto> Handle(GetTestStartInfoQuery request, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepository.GetByIdWithDetailsAsync(request.AssignmentId, cancellationToken)
                         ?? throw new NotFoundException(nameof(TestAssignment), request.AssignmentId);

        // Hata durumları için DTO'yu doğru sırada boş değerlerle doldur
        if (assignment.Status == TestAssignmentStatus.Completed)
        {
            return new TestStartInfoDto("", "", null, null, null, false, "This test has already been completed.", null);
        }

        if (assignment.ExpiresAt < DateTime.UtcNow)
        {
            return new TestStartInfoDto("", "", null, null, null, false, "The deadline for this test has expired.", null);
        }

        // Başarılı durumda, DTO'yu doğru sırada ve doğru değerlerle doldur
        return new TestStartInfoDto(
            CandidateFullName: assignment.Candidate.FullName,
            TestTitle: assignment.TestDefinition.Title,
            TestDescription: assignment.TestDefinition.Description,
            QuestionCount: assignment.QuestionCount,
            TimeLimitInMinutes: assignment.TimeLimitInMinutes,
            CanStart: true,
            StatusMessage: "Ready to start the test.",
            StartedAt: assignment.StartedAt
        );
    }
}