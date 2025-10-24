using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Enums;
using Hangfire;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Commands.Create;
// src/Application/Features/TestAssignments/Commands/Create/AssignTestCommandHandler.cs
public class AssignTestCommandHandler : IRequestHandler<AssignTestCommand, List<string>>
{
    // Artık _candidateRepository'ye ihtiyacımız yok
    private readonly ITestAssignmentRepository _assignmentRepository;
    private readonly ITestDefinitionRepository _testDefinitionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRepository<Candidate> _candidateRepo; // Aday bilgisini almak için

    public AssignTestCommandHandler(ITestAssignmentRepository assignmentRepository, ITestDefinitionRepository testDefinitionRepository, IUnitOfWork unitOfWork, IBackgroundJobClient backgroundJobClient, IRepository<Candidate> candidateRepo)
    {
        _assignmentRepository = assignmentRepository;
        _testDefinitionRepository = testDefinitionRepository;
        _unitOfWork = unitOfWork;
        _backgroundJobClient = backgroundJobClient;
        _candidateRepo = candidateRepo;
    }


    public async Task<List<string>> Handle(AssignTestCommand request, CancellationToken cancellationToken)
    {
      

     
        var testDefinition = await _testDefinitionRepository.GetByIdWithQuestionsAsync(request.TestDefinitionId, cancellationToken)
                             ?? throw new NotFoundException("Test tanımı bulunamadı.", request.TestDefinitionId);


        var createdAssignmentIds = new List<string>();
        var expirationDate = DateTime.UtcNow.AddDays(request.DaysToExpire);

        // Gelen ID listesindeki her aday için işlem yap
        foreach (var candidateId in request.CandidateIds)
        {
            var candidate = await _candidateRepo.GetByIdAsync(candidateId, cancellationToken);
            if (candidate == null) continue; // Aday bulunamazsa atla

            var assignment = new TestAssignment
            {
                CandidateId = candidate.Id,
                TestDefinitionId = request.TestDefinitionId,
                Status = TestAssignmentStatus.Pending,
                ExpiresAt = expirationDate,
                Language = request.Language,
                TimeLimitInMinutes = request.TimeLimitInMinutes ?? testDefinition.DefaultTimeLimitInMinutes ?? 0,
                QuestionCount = request.QuestionCount ?? testDefinition.DefaultQuestionCount ?? testDefinition.MultipleChoiceQuestions.Count
               
            };
            await _assignmentRepository.AddAsync(assignment, cancellationToken);
            createdAssignmentIds.Add(assignment.Id);

            var testLink = $"http://localhost:5173/take-test/{assignment.Id}";
            _backgroundJobClient.Enqueue<IEmailService>(emailService =>
                emailService.SendTestInvitationEmailAsync(
                    candidate.Email, $"{candidate.FirstName} {candidate.LastName}",
                    testDefinition.Title, testLink, expirationDate));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return createdAssignmentIds;
    }
}