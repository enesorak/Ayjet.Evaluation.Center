namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Queries.GetTestProgress;

public record QuestionProgressDto(
    int QuestionId,
    string QuestionText,
    List<AnswerOptionProgressDto> Options,
    int? SelectedAnswerOptionId // Adayın bu soruya verdiği cevabın ID'si (null ise cevaplanmamış)
);