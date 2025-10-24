using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.CandidateAnswers.Commands.Submit;

public record SubmitAnswerCommand(
    string AssignmentId,
    int QuestionId,
    int? SelectedOptionId, // Çoktan seçmeli için
    int? PsychometricResponse // Psikometrik için (0,1,2)
) : IRequest;