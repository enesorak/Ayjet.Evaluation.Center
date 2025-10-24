using System.ComponentModel.DataAnnotations.Schema;
using Ayjet.Evaluation.Center.Domain.Common;
using Ayjet.Evaluation.Center.Domain.Enums;

namespace Ayjet.Evaluation.Center.Domain.Entities;

public class Candidate : BaseEntityWithGuid
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string? ProfilePictureUrl { get; set; } // <-- BU SATIRI EKLEYİN
    
    public DateOnly? BirthDate { get; set; } // <-- ADD THIS

    public Department Department { get; set; }
    public CandidateType CandidateType { get; set; }
    public string? InitialCode { get; set; }
    public string? FleetCode { get; set; }
    public bool IsArchived { get; set; } = false; // Arşiv durumu
    
    // MMPI için demografik bilgiler
    public Gender? Gender { get; set; }
    public string? MaritalStatus { get; set; } // Örn: "Evli", "Bekar"
    public string? Profession { get; set; } // Meslek
    public string? EducationLevel { get; set; } // Eğitim Seviyesi
    
    public bool IsProfileConfirmed { get; set; } = false;
    public DateTime? ProfileConfirmedAt { get; set; }
    
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}"; // <-- Bu satırı ekleyin

    
    
    public ICollection<TestAssignment> TestAssignments { get; set; } = new List<TestAssignment>();
}