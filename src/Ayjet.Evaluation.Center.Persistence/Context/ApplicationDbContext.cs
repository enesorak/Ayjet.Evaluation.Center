using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ayjet.Evaluation.Center.Persistence.Context;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IUnitOfWork 
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Domain katmanında oluşturduğumuz her bir varlık için bir DbSet ekliyoruz.
    // DbSet<T>, veritabanındaki bir tabloyu temsil eder.
    public DbSet<TestDefinition> TestDefinitions { get; set; }
    public DbSet<MultipleChoiceQuestion> MultipleChoiceQuestions { get; set; }
    public DbSet<MultipleChoiceQuestionTranslation> MultipleChoiceQuestionTranslations { get; set; }
    public DbSet<AnswerOption> AnswerOptions { get; set; }
    public DbSet<AnswerOptionTranslation> AnswerOptionTranslations { get; set; }
    
    public DbSet<PsychometricQuestion> PsychometricQuestions { get; set; }
    public DbSet<PsychometricQuestionTranslation> PsychometricQuestionTranslations { get; set; }

    
    public DbSet<Candidate> Candidates { get; set; }
    public DbSet<TestAssignment> TestAssignments { get; set; }
    
    public DbSet<TestResult> TestResults { get; set; }

    public DbSet<CandidateAnswer> CandidateAnswers { get; set; }
    
    
    public DbSet<PsychometricScale> PsychometricScales { get; set; }
    
    
    public DbSet<QuestionScaleMapping> QuestionScaleMappings { get; set; }
    
 
    
    
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // İlişkilerin, anahtarların ve kısıtlamaların (constraints)
        // otomatik olarak algılanamadığı durumlarda, burada manuel olarak
        // konfigürasyon yapabiliriz. Şimdilik EF Core'un varsayılan
        // konvansiyonları bizim için yeterli olacaktır.
        base.OnModelCreating(modelBuilder);
        
        // Örnek: modelBuilder.Entity<TestDefinition>().HasKey(t => t.Id);


        modelBuilder.Entity<Candidate>()
            .HasIndex(c => c.InitialCode)
            .IsUnique()
            .HasFilter("\"InitialCode\" IS NOT NULL");;
        
       
        modelBuilder.Entity<QuestionScaleMapping>()
            .HasIndex(qsm => new { 
                qsm.PsychometricQuestionId, 
                qsm.PsychometricScaleId, 
                qsm.RequiredGender 
            })
            .IsUnique();
    }}