namespace Ayjet.Evaluation.Center.Application.Features.Candidates.Queries.GetList;

public class CandidateDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public string CandidateType { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string? InitialCode { get; set; }
    public string? FleetCode { get; set; }
    public DateOnly? BirthDate { get; set; } // Yeni alan
    public int? Age => BirthDate.HasValue
        ? DateTime.Today.Year - BirthDate.Value.Year - (DateTime.Today.DayOfYear < BirthDate.Value.DayOfYear ? 1 : 0)
        : null; // Otomatik hesaplanan yaş
    public string? Gender { get; set; }
    public string? MaritalStatus { get; set; }
    public string? Profession { get; set; }
    public string? EducationLevel { get; set; }
    public bool IsArchived { get; set; }
    public bool IsProfileConfirmed { get; set; } = false;
}