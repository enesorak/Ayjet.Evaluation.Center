using Ayjet.Evaluation.Center.Domain.Enums;

namespace Ayjet.Evaluation.Center.Application.Features.Candidates.DTOs;

public class UpdateProfileDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? InitialCode { get; set; }
    public string? FleetCode { get; set; }
    public string? PhoneNumber { get; set; }
    
  //  public int CandidateType { get; set; } // <-- YENİ ALAN
   // public int Department { get; set; }    // <-- YENİ ALAN
    public Gender? Gender { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? MaritalStatus { get; set; }
    public string? Profession { get; set; }
    public string? EducationLevel { get; set; }
}