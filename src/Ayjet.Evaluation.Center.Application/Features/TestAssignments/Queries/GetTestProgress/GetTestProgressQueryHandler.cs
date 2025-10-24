using AutoMapper;
using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Application.Features.Candidates.Queries.GetList;
using Ayjet.Evaluation.Center.Domain.Common;
using Ayjet.Evaluation.Center.Domain.Entities;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Queries.GetTestProgress;

public class GetTestProgressQueryHandler(ITestAssignmentRepository assignmentRepository, IMapper mapper)
    : IRequestHandler<GetTestProgressQuery, TestProgressDto>
{
     public async Task<TestProgressDto> Handle(GetTestProgressQuery request, CancellationToken cancellationToken)
    {
        var assignment = await assignmentRepository.GetProgressAsync(request.AssignmentId, cancellationToken)
            ?? throw new NotFoundException(nameof(TestAssignment), request.AssignmentId);

        // --- DÜZELTME: GEREKSİZ HATA FIRLATMA KODU KALDIRILDI ---
        // Artık bu handler, testin durumu ne olursa olsun (Completed, Expired vb.)
        // bilgileri frontend'e her zaman döndürecek. Kararı frontend verecek.
        
        var answersLookup = assignment.CandidateAnswers.ToDictionary(a => (a.MultipleChoiceQuestionId ?? a.PsychometricQuestionId)!.Value);
        var questionsDto = new List<QuestionProgressDto>();
        var targetLanguage = assignment.Language;

        var orderedQuestions = assignment.AssignedQuestions
            .Select(aq => (aq.PsychometricQuestion ?? (object)aq.MultipleChoiceQuestion)!)
            .OfType<IQuestion>()
            .OrderBy(q => q.DisplayOrder);

        foreach (var qBase in orderedQuestions)
        {
            if (qBase is PsychometricQuestion q)
            {
                answersLookup.TryGetValue(q.Id, out var answer);
                questionsDto.Add(new QuestionProgressDto(
                    QuestionId: q.Id,
                    QuestionText: q.Translations.FirstOrDefault(t => t.Language == targetLanguage)?.Text ?? "N/A",
                    Options: GetLocalizedPsychometricOptions(targetLanguage),
                    SelectedAnswerOptionId: (int?)answer?.PsychometricResponse
                ));
            }
            else if (qBase is MultipleChoiceQuestion mcq)
            {
                answersLookup.TryGetValue(mcq.Id, out var answer);
                questionsDto.Add(new QuestionProgressDto(
                    QuestionId: mcq.Id,
                    QuestionText: mcq.Translations.FirstOrDefault(t => t.Language == targetLanguage)?.Text ?? "N/A",
                    Options: mcq.Options.Select(o => new AnswerOptionProgressDto(o.Id, o.Translations.FirstOrDefault(t => t.Language == targetLanguage)?.Text ?? "N/A")).ToList(),
                    SelectedAnswerOptionId: answer?.SelectedOptionId
                ));
            }
        }
        
        return new TestProgressDto
        {
            AssignmentId = assignment.Id,
            Candidate = mapper.Map<CandidateDto>(assignment.Candidate),
            TestTitle = assignment.TestDefinition.Title,
            TimeLimitInMinutes = assignment.TimeLimitInMinutes,
            StartedAt = assignment.StartedAt,
            TestType = assignment.TestDefinition.Type,
            Status = assignment.Status.ToString(),
            Questions = questionsDto
        };
    }

    private List<AnswerOptionProgressDto> GetLocalizedPsychometricOptions(string language)
    {
        bool isEnglish = language.StartsWith("en", StringComparison.OrdinalIgnoreCase);

        return new List<AnswerOptionProgressDto>
        {
            new(1, isEnglish ? "True" : "Doğru"),
            new(2, isEnglish ? "False" : "Yanlış"),
            new(0, isEnglish ? "I don't know" : "Fikrim Yok")
        };
    }
}