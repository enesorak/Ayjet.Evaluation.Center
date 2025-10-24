using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Enums;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;


// Namespace'in doğru olduğundan emin ol (Ayjet.Evaluation.Center...)
namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Commands.ImportMmpi;

public class ImportMmpiAnswersCommandHandler : IRequestHandler<ImportMmpiAnswersCommand, string>
{
    private readonly ICandidateRepository _candidateRepo;
    private readonly ITestDefinitionRepository _testDefinitionRepo;
    private readonly IPsychometricQuestionRepository _questionRepo;
    private readonly ITestAssignmentRepository _assignmentRepo;
    private readonly ICandidateAnswerRepository _answerRepo;
    private readonly IRepository<AssignmentQuestion> _assignmentQuestionRepo; // <-- Düzeltilmiş bağımlılık
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public ImportMmpiAnswersCommandHandler(
        ICandidateRepository candidateRepo,
        ITestDefinitionRepository testDefinitionRepo,
        IPsychometricQuestionRepository questionRepo,
        ITestAssignmentRepository assignmentRepo,
        ICandidateAnswerRepository answerRepo,
        IRepository<AssignmentQuestion> assignmentQuestionRepo, // <-- Constructor'a eklendi
        IUnitOfWork unitOfWork,
        IBackgroundJobClient backgroundJobClient)
    {
        _candidateRepo = candidateRepo;
        _testDefinitionRepo = testDefinitionRepo;
        _questionRepo = questionRepo;
        _assignmentRepo = assignmentRepo;
        _answerRepo = answerRepo;
        _assignmentQuestionRepo = assignmentQuestionRepo; // <-- Atama yapıldı
        _unitOfWork = unitOfWork;
        _backgroundJobClient = backgroundJobClient;
    }

    public async Task<string> Handle(ImportMmpiAnswersCommand request, CancellationToken cancellationToken)
    {
        // 1. Cevapları CSV Dosyasından Oku
        var answers = new Dictionary<int, int>();
        try
        {
            answers = await ParseAnswersFromCsv(request.AnswerFile, cancellationToken);
            if (!answers.Any())
            {
                
                throw new ValidationException("CSV file is empty or does not contain valid answers.");
            }
        }
        catch (Exception ex)
        {
            throw new ValidationException($"Error parsing CSV file: {ex.Message}");
        }

        // 2. Gerekli Varlıkları Bul
        var candidate = await _candidateRepo.GetByIdAsync(request.CandidateId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidate), request.CandidateId);

        var mmpiTestDefinition = (await _testDefinitionRepo.ListAsync(td => td.Title == "MMPI", cancellationToken)).FirstOrDefault()
            ?? throw new NotFoundException("MMPI test definition not found.");

        // 3. Yeni Bir Test Ataması Oluştur
        var assignment = new TestAssignment
        {
            CandidateId = candidate.Id,
            TestDefinitionId = mmpiTestDefinition.Id,
            Status = TestAssignmentStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddYears(1),
            Language = mmpiTestDefinition.Language,
            CreatedAt = request.CompletedAtOverride?.AddMinutes(-30) ?? DateTime.UtcNow.AddMinutes(-30)
        };
        await _assignmentRepo.AddAsync(assignment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken); // ID'nin oluşması için kaydet

        // 4. Tüm MMPI Sorularını Al ve Eşleme Yap
        var allMmpiQuestions = await _questionRepo.GetAllByTestDefinitionIdAsync(mmpiTestDefinition.Id);
        var questionMap = allMmpiQuestions.ToDictionary(q => q.DisplayOrder, q => q.Id);

        // 5. Atamaya Tüm Soruları Ekle (Repository Kullanarak)
        var assignmentQuestions = allMmpiQuestions
            .Select(q => new AssignmentQuestion { TestAssignmentId = assignment.Id, PsychometricQuestionId = q.Id })
            .ToList();
        // --- Düzeltme: Repository kullanıyoruz ---
        await _assignmentQuestionRepo.AddRangeAsync(assignmentQuestions, cancellationToken);
        // ------------------------------------

        // 6. Okunan Cevapları Kaydet
        var candidateAnswers = new List<CandidateAnswer>();
        var now = DateTime.UtcNow;
        foreach (var answerPair in answers)
        {
            if (questionMap.TryGetValue(answerPair.Key, out var questionId))
            {
                candidateAnswers.Add(new CandidateAnswer
                {
                    TestAssignmentId = assignment.Id,
                    PsychometricQuestionId = questionId,
                    PsychometricResponse = answerPair.Value,
                    AnsweredAt = request.CompletedAtOverride ?? now,
                    IsCorrectAtTimeOfAnswer = false,
                    QuestionSnapshotJson = "{}",
                    CreatedAt = now
                });
            }
            else
            {
                 Console.WriteLine($"Warning: Question number {answerPair.Key} not found in MMPI definition.");
            }
        }
        await _answerRepo.AddRangeAsync(candidateAnswers, cancellationToken);

        // 7. Atamanın Durumunu Güncelle
        assignment.Status = TestAssignmentStatus.Completed;
        assignment.StartedAt = request.CompletedAtOverride?.AddMinutes(-30) ?? now.AddMinutes(-30);
        assignment.CompletedAt = request.CompletedAtOverride ?? now;
        assignment.UpdatedAt = now;
        _assignmentRepo.Update(assignment);

        // 8. Tüm Değişiklikleri Veritabanına Kaydet
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 9. Puanlama İşini Arka Plana Gönder
        _backgroundJobClient.Enqueue<IScoringService>(service =>
            service.CalculateAndSaveScoreAsync(assignment.Id));

        return assignment.Id;
    }

    // --- CSV OKUMA METODU ---
    private async Task<Dictionary<int, int>> ParseAnswersFromCsv(IFormFile file, CancellationToken cancellationToken)
    {
        var answers = new Dictionary<int, int>();
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null,
            TrimOptions = TrimOptions.Trim,
            PrepareHeaderForMatch = args => args.Header.ToLower()
        };

        using var reader = new StreamReader(file.OpenReadStream());
        using var csv = new CsvReader(reader, config);
        var records = csv.GetRecordsAsync<dynamic>(cancellationToken);

        await foreach (var record in records)
        {
            var recordDict = (IDictionary<string, object>)record;
            if (recordDict.TryGetValue("questionid", out var questionIdObj) &&
                recordDict.TryGetValue("answer", out var answerObj) &&
                int.TryParse(questionIdObj?.ToString(), out int questionId) &&
                int.TryParse(answerObj?.ToString(), out int answer) &&
                answer >= 0 && answer <= 2)
            {
                answers[questionId] = answer;
            }
        }
        return answers;
    }
}