using Ayjet.Evaluation.Center.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AAyjet.Evaluation.Center.Persistence.DataSeeders;

public static class IdentityDataSeeder
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Rolleri oluştur
        string[] roleNames = { 
            "Admin", 
            "QuestionManager.MultipleChoice",
            "TestAssigner.MultipleChoice",
            "TestAssigner.Psychometric",
            "ReportViewer.MultipleChoice",
            "ReportViewer.Psychometric",
            "Candidate"
        };
        
        
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
        var adminEmail = "orak.enes@gmail.com";
        // Super Admin kullanıcısını oluştur
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            var newAdminUser = new ApplicationUser
            {
                UserName = adminEmail, // UserName olarak da e-postayı kullanmak en iyisi
                Email = adminEmail,
                FirstName = "Enes",
                LastName = "Orak",
                EmailConfirmed = true
            };
            
            // Şifre kurallarına uyan bir şifre belirleyin
            var result = await userManager.CreateAsync(newAdminUser, "A12345e."); 
            if (result.Succeeded)
            {
                // Admin rolüne ata
                await userManager.AddToRoleAsync(newAdminUser, "Admin");
            }
        }
    }
}