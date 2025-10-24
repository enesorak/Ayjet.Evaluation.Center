using System.Text.Json;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Enums;

namespace Ayjet.Evaluation.Center.Application.Services.Scoring;

public class MultipleChoiceScoringStrategy : IScoringStrategy
{
    public TestType Handles => TestType.MultipleChoice;

    public Task<TestResult> ScoreAsync(TestAssignment assignment)
    {
        var totalQuestions = assignment.QuestionCount ?? assignment.TestDefinition.DefaultQuestionCount ?? 0;
        if (totalQuestions == 0)
        {
            // Eğer teste atanmış hiç soru yoksa, skor 0'dır.
            return Task.FromResult(new TestResult 
            { 
                Score = 0, 
                TestAssignmentId = assignment.Id,
                ResultsPayloadJson = "{ \"correct\": 0, \"total\": 0, \"incorrect\": 0 }"
            });
        }
  
        var correctAnswers = assignment.CandidateAnswers.Count(a => a.IsCorrectAtTimeOfAnswer);

        var score = totalQuestions > 0 ? ((decimal)correctAnswers / totalQuestions) * 100 : 0;

        var resultPayload = new 
        {
            correct = correctAnswers,
            total = totalQuestions,
            incorrect = totalQuestions - correctAnswers
        };
        var result = new TestResult

        {
            TestAssignmentId = assignment.Id,
            Score = Math.Round(score, 2),
            ResultsPayloadJson = JsonSerializer.Serialize(resultPayload),
            CreatedAt = DateTime.UtcNow
        };
    
        return Task.FromResult(result);
 
    }
}