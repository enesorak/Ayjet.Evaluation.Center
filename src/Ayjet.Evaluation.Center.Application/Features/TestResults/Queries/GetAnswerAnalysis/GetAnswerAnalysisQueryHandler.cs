using System.Text.Json;
using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Application.Features.TestResults.Queries.GetByAssignmentId;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Enums;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestResults.Queries.GetAnswerAnalysis;

public class GetAnswerAnalysisQueryHandler : IRequestHandler<GetAnswerAnalysisQuery, List<AnswerDetailDto>>
{
    private readonly ITestAssignmentRepository _assignmentRepo;

    public GetAnswerAnalysisQueryHandler(ITestAssignmentRepository assignmentRepo)
    {
        _assignmentRepo = assignmentRepo;
    }

    public async Task<List<AnswerDetailDto>> Handle(GetAnswerAnalysisQuery request, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepo.GetProgressAsync(request.AssignmentId, cancellationToken)
            ?? throw new NotFoundException("Test ataması bulunamadı.", request.AssignmentId);

        var answerDetails = new List<AnswerDetailDto>();
        var targetLanguage = assignment.Language;
        var isPsychometric = assignment.TestDefinition.Type == TestType.Psychometric;

        var allQuestions = isPsychometric
            ? assignment.AssignedQuestions.Where(aq => aq.PsychometricQuestion != null).Select(aq => (object)aq.PsychometricQuestion!)
            : assignment.AssignedQuestions.Where(aq => aq.MultipleChoiceQuestion != null).Select(aq => (object)aq.MultipleChoiceQuestion!);

        foreach (var qBase in allQuestions.OrderBy(q => ((dynamic)q).DisplayOrder))
        {
            if (qBase is PsychometricQuestion pq)
            {
                var answer = assignment.CandidateAnswers.FirstOrDefault(a => a.PsychometricQuestionId == pq.Id);
                var responseText = GetPsychometricResponseText(answer?.PsychometricResponse, targetLanguage);
                
                answerDetails.Add(new AnswerDetailDto(
                    QuestionText: pq.Translations.FirstOrDefault(t => t.Language == targetLanguage)?.Text ?? "N/A",
                    YourAnswer: responseText,
                    CorrectAnswer: "-",
                    WasCorrect: true
                ));
            }
            else if (qBase is MultipleChoiceQuestion mcq)
            {
                var answer = assignment.CandidateAnswers.FirstOrDefault(a => a.MultipleChoiceQuestionId == mcq.Id);
                string yourAnswerText = "Cevaplanmamış";
                string correctAnswerText = mcq.Options.FirstOrDefault(o => o.IsCorrect)?.Translations.FirstOrDefault(t => t.Language == targetLanguage)?.Text ?? "N/A";
                
                if (answer != null && !string.IsNullOrEmpty(answer.QuestionSnapshotJson))
                {
                    var snapshot = JsonSerializer.Deserialize<QuestionSnapshotDto>(answer.QuestionSnapshotJson);
                    var selectedOption = mcq.Options.FirstOrDefault(o => o.Id == snapshot?.SelectedOptionId);
                    yourAnswerText = selectedOption?.Translations.FirstOrDefault(t => t.Language == targetLanguage)?.Text ?? "Cevap bulunamadı";
                }
                
                answerDetails.Add(new AnswerDetailDto(
                    QuestionText: mcq.Translations.FirstOrDefault(t => t.Language == targetLanguage)?.Text ?? "N/A",
                    YourAnswer: yourAnswerText,
                    CorrectAnswer: correctAnswerText,
                    WasCorrect: answer?.IsCorrectAtTimeOfAnswer ?? false
                ));
            }
        }
        
        return answerDetails;
    }
    
    private string GetPsychometricResponseText(int? response, string language)
    {
        var isEnglish = language == "en-US";
        return response switch
        {
            1 => isEnglish ? "True" : "Doğru",
            2 => isEnglish ? "False" : "Yanlış",
            0 => isEnglish ? "No Idea" : "Fikrim Yok",
            _ => "Cevaplanmamış"
        };
    }
}