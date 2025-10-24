namespace Ayjet.Evaluation.Center.Domain.Entities;

public class AssignmentQuestion
{
    public int Id { get; set; } // Basit bir ID
    public string TestAssignmentId { get; set; } = null!;
    public TestAssignment TestAssignment { get; set; }
    public int? MultipleChoiceQuestionId { get; set; }
    public MultipleChoiceQuestion? MultipleChoiceQuestion { get; set; }
    
    
    public int? PsychometricQuestionId { get; set; }
    public PsychometricQuestion? PsychometricQuestion { get; set; }
}