using System.Text.Json;
using AutoMapper;
using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestResults.Queries.GetByAssignmentId;

 public class GetTestResultByAssignmentIdQueryHandler : IRequestHandler<GetTestResultByAssignmentIdQuery, TestResultDto>
{
    private readonly ITestResultRepository _resultRepository;
    private readonly IMapper _mapper;

    public GetTestResultByAssignmentIdQueryHandler(ITestResultRepository resultRepository, IMapper mapper)
    {
        _resultRepository = resultRepository;
        _mapper = mapper;
    }

    public async Task<TestResultDto> Handle(GetTestResultByAssignmentIdQuery request, CancellationToken cancellationToken)
    {
        var testResult = await _resultRepository.GetByAssignmentIdWithDetailsAsync(request.AssignmentId, cancellationToken)
            ?? throw new NotFoundException("Sınav sonucu bulunamadı.");

        // AutoMapper ile temel bilgileri (Aday Adı, Test Adı vb.) map'le
        var resultDto = _mapper.Map<TestResultDto>(testResult);

        // Cevap detaylarını JSON snapshot'tan oku
        var answerDetails = new List<AnswerDetailDto>();

        // Önce tüm soruları bir sözlüğe alalım (veritabanından gelen)
        var questionsMap = testResult.TestAssignment.TestDefinition.MultipleChoiceQuestions
                            .ToDictionary(q => q.Id);

        // Adayın cevaplarını, soruların doğru sırasına göre işle
        foreach (var question in questionsMap.Values.OrderBy(q => q.DisplayOrder))
        {
            var candidateAnswer = testResult.TestAssignment.CandidateAnswers
                .FirstOrDefault(a => a.MultipleChoiceQuestionId == question.Id);

            if (candidateAnswer != null && !string.IsNullOrEmpty(candidateAnswer.QuestionSnapshotJson))
            {
                // Cevap varsa, snapshot'ı çöz
                var snapshot = JsonSerializer.Deserialize<QuestionSnapshotDto>(candidateAnswer.QuestionSnapshotJson);
                var selectedOptionText = snapshot?.Options.FirstOrDefault(o => o.Id == snapshot.SelectedOptionId)?.Text ?? "Seçim bulunamadı";
                var correctOptionText = snapshot?.Options.FirstOrDefault(o => o.IsCorrect)?.Text ?? "Doğru cevap tanımsız";

                answerDetails.Add(new AnswerDetailDto(
                    QuestionText: snapshot?.QuestionText ?? "Soru metni alınamadı",
                    YourAnswer: selectedOptionText,
                    CorrectAnswer: correctOptionText,
                    WasCorrect: candidateAnswer.IsCorrectAtTimeOfAnswer
                ));
            }
            else
            {
                // Aday bu soruyu cevaplamamışsa
                var correctOptionText = question.Options.FirstOrDefault(o => o.IsCorrect)?.Translations.FirstOrDefault()?.Text;
                answerDetails.Add(new AnswerDetailDto(
                    QuestionText: question.Translations.FirstOrDefault()?.Text ?? "N/A",
                    YourAnswer: "Cevaplanmamış",
                    CorrectAnswer: correctOptionText ?? "N/A",
                    WasCorrect: false
                ));
            }
        }

        resultDto.AnswerDetails = answerDetails;
        return resultDto;
    }
}