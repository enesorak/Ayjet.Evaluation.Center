using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Enums;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Commands.Start;

public class StartTestCommandHandler(
    ITestAssignmentRepository assignmentRepo,
    IUnitOfWork unitOfWork,
    IMultipleChoiceQuestionRepository mcqRepo,
    IPsychometricQuestionRepository psychometricRepo)
    : IRequestHandler<StartTestCommand>
{
    public async Task Handle(StartTestCommand request, CancellationToken cancellationToken)
    {
        var assignment = await assignmentRepo.GetByIdWithDetailsAsync(request.AssignmentId, cancellationToken)
                         
                         
            ?? throw new NotFoundException(nameof(TestAssignment), request.AssignmentId);

        // --- EXCEPTION DİLİ GÜNCELLENDİ ---
        if (assignment.Status != TestAssignmentStatus.Pending)
            throw new Exception("This test has already been started or completed.");

        if (assignment.Candidate is { IsProfileConfirmed: false })
            throw new Exception("Candidate profile must be confirmed before starting the test.");

        assignment.Status = TestAssignmentStatus.InProgress;
        assignment.StartedAt = DateTime.UtcNow;
        assignment.AssignedQuestions.Clear();

        var testDef = assignment.TestDefinition;
        var questionCount = assignment.QuestionCount ?? testDef.DefaultQuestionCount ?? 0;

        // --- YENİ VE DOĞRU MANTIK ---
        if (testDef.Type == TestType.Psychometric)
        {
            // Psikometrik ise, TÜM soruları sırasıyla ata
            var allQuestions = await psychometricRepo.GetAllByTestDefinitionIdAsync(testDef.Id);
            foreach (var question in allQuestions)
            {
                assignment.AssignedQuestions.Add(new AssignmentQuestion { PsychometricQuestionId = question.Id });
            }
        }
        else // MultipleChoice
        {
            // Çoktan seçmeli ise, veritabanında rastgele seçerek ata (PERFORMANS İYİLEŞTİRMESİ)
            var questionsToTake = await mcqRepo.GetRandomQuestionsAsync(testDef.Id, questionCount, cancellationToken);
            foreach(var question in questionsToTake)
            {
                assignment.AssignedQuestions.Add(new AssignmentQuestion { MultipleChoiceQuestionId = question.Id });
            }
        }
        // ------------------------------
        
        assignmentRepo.Update(assignment);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}