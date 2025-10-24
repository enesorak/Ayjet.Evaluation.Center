namespace Ayjet.Evaluation.Center.Application.Features.TestResults.Queries.GetByAssignmentId;

public record AnswerDetailDto(
    string QuestionText,
    string YourAnswer,
    string CorrectAnswer,
    bool WasCorrect
);