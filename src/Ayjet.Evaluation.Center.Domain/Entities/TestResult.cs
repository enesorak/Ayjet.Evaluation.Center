using Ayjet.Evaluation.Center.Domain.Common;

namespace Ayjet.Evaluation.Center.Domain.Entities;

public class TestResult : BaseEntity<int>
{
    public string TestAssignmentId { get; set; }
    public TestAssignment TestAssignment { get; set; }

    public decimal Score { get; set; } // Örn: 85.5
    public string? ResultsPayloadJson { get; set; } // Detaylı KPI'lar için esnek JSON alanı
}