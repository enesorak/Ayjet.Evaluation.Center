using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;

namespace Ayjet.Evaluation.Center.Application.Services.Scoring;

public class ScoringService : IScoringService
{
    private readonly IEnumerable<IScoringStrategy> _strategies;
    private readonly IRepository<TestResult> _testResultRepository;
   
    private readonly INotificationService _notificationService;

    private readonly ITestAssignmentRepository _assignmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ScoringService(IEnumerable<IScoringStrategy> strategies, IRepository<TestResult> testResultRepository, INotificationService notificationService, ITestAssignmentRepository assignmentRepository, IUnitOfWork unitOfWork)
    {
        _strategies = strategies;
        _testResultRepository = testResultRepository;
        _notificationService = notificationService;
        _assignmentRepository = assignmentRepository;
        _unitOfWork = unitOfWork;
    }
    // Artık UnitOfWork'e burada ihtiyacımız yok

    

    
    
    public async Task CalculateAndSaveScoreAsync(string assignmentId)
    {
        // Puanlanacak olan TestAssignment'ı tüm detaylarıyla getir
        var assignment = await _assignmentRepository.GetForScoringAsync(assignmentId)
                         ?? throw new NotFoundException(nameof(TestAssignment), assignmentId);

        var strategy = _strategies.FirstOrDefault(s => s.Handles == assignment.TestDefinition.Type)
                       ?? throw new Exception($"No scoring strategy found for test type {assignment.TestDefinition.Type}");

        // Stratejiyi çağır ve sonucu al
      
        var newTestResultData = await strategy.ScoreAsync(assignment);
        
        var existingResult = await _testResultRepository.FirstOrDefaultAsync(r => r.TestAssignmentId == assignmentId);


        if (existingResult != null)
        {
            // Varsa, sadece skor ve detayları güncelle
            existingResult.Score = newTestResultData.Score;
            existingResult.ResultsPayloadJson = newTestResultData.ResultsPayloadJson;
            existingResult.UpdatedAt = DateTime.UtcNow;
            _testResultRepository.Update(existingResult);
        }
        else
        {
            // Yoksa, yeni bir kayıt olarak ekle
            await _testResultRepository.AddAsync(newTestResultData);
        }
        // ------------------------------------------

        await _unitOfWork.SaveChangesAsync();
        
        
        // --- YENİ BİLDİRİM GÖNDERME KODU ---
        // Puanlama bitince, "Admin" grubundaki herkese haber ver.
        var candidateName = assignment.Candidate.FullName;
        var message = $"'{candidateName}' adlı adayın sınavı yeniden puanlandı. Raporu görmek için sayfayı yenileyebilirsiniz.";

        await _notificationService.SendScoreUpdateNotificationAsync(message, assignmentId);
       
    }
}