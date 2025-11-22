using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Enums;
using Hangfire;
using MediatR;

 

 
namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Commands.Finish;

public class FinishTestCommandHandler : IRequestHandler<FinishTestCommand>
{
    private readonly ITestAssignmentRepository _assignmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICandidateAnswerRepository _answerRepository; // <-- EKSİK BAĞIMLILIK
    private readonly IBackgroundJobClient _backgroundJobClient;

    public FinishTestCommandHandler(ITestAssignmentRepository assignmentRepository, IUnitOfWork unitOfWork, IBackgroundJobClient backgroundJobClient, ICandidateAnswerRepository answerRepository)
    {
        _assignmentRepository = assignmentRepository;
        _unitOfWork = unitOfWork;
 
        _backgroundJobClient = backgroundJobClient;
        _answerRepository = answerRepository;
    }

    public async Task Handle(FinishTestCommand request, CancellationToken cancellationToken)
    { 
        var assignment = await _assignmentRepository.GetProgressAsync(request.AssignmentId, cancellationToken);
             
        var now = DateTime.UtcNow;
        
        
       if (assignment.Status != TestAssignmentStatus.InProgress)
        {
            return; // Zaten bitmişse bir şey yapma
        }
       
       

        var isPsychometric = assignment.TestDefinition.Type == TestType.Psychometric;
       

        var candidateAnswers = new List<CandidateAnswer>();

        // Frontend'den gelen cevaplar (Dictionary<string, int?>) üzerinde dön
        foreach (var answerPair in request.Answers)
        {
            // Gelen QuestionId'nin (string) int'e çevrilmesi
            if (!int.TryParse(answerPair.Key, out int questionId))
            {
                continue; // Geçersiz QuestionId formatı, atla
            }

            // Bu sorunun bu sınava atandığını doğrula
            var assignedQuestion = assignment.AssignedQuestions
                .FirstOrDefault(aq => aq.MultipleChoiceQuestionId == questionId || aq.PsychometricQuestionId == questionId);

            if (assignedQuestion == null)
            {
                // Adaya atanmamış bir soruyu cevaplamaya çalışıyorsa atla (Güvenlik)
                continue; 
            }

            var candidateAnswer = new CandidateAnswer
            {
                TestAssignmentId = assignment.Id,
                AnsweredAt = now,
                CreatedAt = now
            };

            // Test tipine göre doğru ID'yi ve cevabı ata
            if (isPsychometric)
            {
                candidateAnswer.PsychometricQuestionId = questionId;
                candidateAnswer.PsychometricResponse = answerPair.Value; // 0, 1, or 2
            }
            else
            {
                candidateAnswer.MultipleChoiceQuestionId = questionId;
                candidateAnswer.SelectedOptionId = answerPair.Value; // Seçenek ID'si
            }
            
            candidateAnswers.Add(candidateAnswer);
        }

        // --- 1. CEVAPLARI VERİTABANINA TOPLU EKLE ---
        if (candidateAnswers.Any())
        {
            await _answerRepository.AddRangeAsync(candidateAnswers, cancellationToken);
        }

        var assignmentToUpdate = await _assignmentRepository
            .GetByIdForUpdateAsync(request.AssignmentId, cancellationToken);

        if (assignmentToUpdate != null)
        {
            assignmentToUpdate.Status = TestAssignmentStatus.Completed;
            assignmentToUpdate.CompletedAt = now;
            assignmentToUpdate.UpdatedAt = now;
            
            // İlişkili entity'ler olmadan sadece assignment'ı güncelle
            _assignmentRepository.Update(assignmentToUpdate);
        }
        // 2. Atamanın durumunu ve bitiş zamanını güncelle
    
        // 3. TÜM DEĞİŞİKLİKLERİ (Cevaplar + Atama Durumu) TEK SEFERDE KAYDET
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 4. Puanlama işini arka plan görevine devret
        _backgroundJobClient.Enqueue<IScoringService>(service => 
            service.CalculateAndSaveScoreAsync(request.AssignmentId));
    }
    
}