using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Enums;

namespace Ayjet.Evaluation.Center.Application.Common.Interfaces;
public interface IScoringStrategy
{
    TestType Handles { get; }
    Task<TestResult> ScoreAsync(TestAssignment assignment);
}