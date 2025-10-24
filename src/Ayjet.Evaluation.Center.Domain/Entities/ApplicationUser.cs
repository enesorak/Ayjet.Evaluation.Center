using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Ayjet.Evaluation.Center.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    public string? ProfilePictureUrl { get; set; }
    
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";
}