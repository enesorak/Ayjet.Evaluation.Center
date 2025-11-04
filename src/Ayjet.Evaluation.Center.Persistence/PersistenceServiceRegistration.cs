using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Persistence.Context;
using Ayjet.Evaluation.Center.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ayjet.Evaluation.Center.Persistence;


public static class PersistenceServiceRegistration
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Geliştirme ortamı için SQLite, Production için PostgreSQL bağlantısını yapılandır
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            // Bu kısmı appsettings.json'a göre daha sonra detaylandıracağız
            options.UseNpgsql(configuration.GetConnectionString("PostgreSQL")); 
        });
        
        
     
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));  

        
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

         
        services.AddScoped<ITestDefinitionRepository, TestDefinitionRepository>();
        services.AddScoped<IMultipleChoiceQuestionRepository, MultipleChoiceQuestionRepository>();  
        
        
        services.AddScoped<ICandidateRepository, CandidateRepository>();
        services.AddScoped<ITestAssignmentRepository, TestAssignmentRepository>();

        services.AddScoped<ICandidateAnswerRepository, CandidateAnswerRepository>();
        services.AddScoped<ITestResultRepository, TestResultRepository>();

        services.AddScoped<IPsychometricQuestionRepository, PsychometricQuestionRepository>();
        return services;
    }
}