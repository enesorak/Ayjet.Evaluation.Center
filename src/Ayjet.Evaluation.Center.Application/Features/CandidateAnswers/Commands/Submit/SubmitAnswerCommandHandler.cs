using System.Text.Json;
using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Enums;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.CandidateAnswers.Commands.Submit;

public class SubmitAnswerCommandHandler : IRequestHandler<SubmitAnswerCommand>
{
    private readonly ITestAssignmentRepository _assignmentRepo;
    private readonly ICandidateAnswerRepository _answerRepo;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitAnswerCommandHandler(
        ITestAssignmentRepository assignmentRepo,
        ICandidateAnswerRepository answerRepo,
        IUnitOfWork unitOfWork)
    {
        _assignmentRepo = assignmentRepo;
        _answerRepo = answerRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SubmitAnswerCommand request, CancellationToken cancellationToken)
    {
        // Snapshot ve doğruluk kontrolü için atamayı tüm detaylarıyla getir
        var assignment = await _assignmentRepo.GetProgressAsync(request.AssignmentId, cancellationToken)
            ?? throw new NotFoundException(nameof(TestAssignment), request.AssignmentId);

        if (assignment.Status != TestAssignmentStatus.InProgress)
        {
            throw new Exception("This test is not currently in progress or has been completed.");
        }

        var existingAnswer = await _answerRepo.FirstOrDefaultAsync(
            a => a.TestAssignmentId == request.AssignmentId &&
                 (a.MultipleChoiceQuestionId == request.QuestionId || a.PsychometricQuestionId == request.QuestionId),
            cancellationToken);

        var isPsychometric = assignment.TestDefinition.Type == TestType.Psychometric;

        if (existingAnswer == null)
        {
            existingAnswer = new CandidateAnswer { CreatedAt = DateTime.UtcNow, TestAssignmentId = request.AssignmentId };
            await _answerRepo.AddAsync(existingAnswer, cancellationToken);
        }
        
        // Ortak alanları güncelle
        existingAnswer.AnsweredAt = DateTime.UtcNow;

        if (isPsychometric)
        {
            var question = assignment.AssignedQuestions
                .Select(aq => aq.PsychometricQuestion)
                .FirstOrDefault(q => q?.Id == request.QuestionId);

            existingAnswer.PsychometricQuestionId = request.QuestionId;
            existingAnswer.PsychometricResponse = request.PsychometricResponse;
            existingAnswer.IsCorrectAtTimeOfAnswer = false; // Psikometrikte tek doğru cevap yok

            var snapshot = new {
                QuestionText = question?.Translations.FirstOrDefault(t => t.Language == assignment.Language)?.Text,
                Response = request.PsychometricResponse 
            };
            existingAnswer.QuestionSnapshotJson = JsonSerializer.Serialize(snapshot);
        }
        else // MultipleChoice
        {
            var question = assignment.AssignedQuestions
                .Select(aq => aq.MultipleChoiceQuestion)
                .FirstOrDefault(q => q?.Id == request.QuestionId);

            if (question != null)
            {
                var selectedOption = question.Options.FirstOrDefault(o => o.Id == request.SelectedOptionId);
                
                existingAnswer.MultipleChoiceQuestionId = request.QuestionId;
                existingAnswer.IsCorrectAtTimeOfAnswer = selectedOption?.IsCorrect ?? false;

                var snapshot = new {
                    QuestionText = question.Translations.FirstOrDefault(t => t.Language == assignment.Language)?.Text,
                    Options = question.Options.Select(o => new { o.Id, Text = o.Translations.FirstOrDefault(t => t.Language == assignment.Language)?.Text, o.IsCorrect }),
                    SelectedOptionId = selectedOption?.Id
                };
                existingAnswer.QuestionSnapshotJson = JsonSerializer.Serialize(snapshot);
            }
        }
        
        // Eğer mevcut bir kaydı güncelliyorsak, EF Core'a bunu bildirmeliyiz.
        // Yeni kayıt ise AddAsync ile zaten takip ediliyor.
        if (existingAnswer.Id != 0)
        {
             _answerRepo.Update(existingAnswer);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}