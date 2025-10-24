namespace Ayjet.Evaluation.Center.Application.Features.MultipleChoiceQuestions.Commands.BulkImport;

public record ImportResultDto(int SuccessCount, int FailedCount, List<string> Errors);