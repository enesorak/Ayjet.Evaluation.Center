using System.Reflection;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Application.Services.Scoring;
using Microsoft.Extensions.DependencyInjection;

namespace Ayjet.Evaluation.Center.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // MediatR'ı Application katmanındaki tüm handler'ları tarayacak şekilde ekle
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        
        // AutoMapper'ı ekle
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddScoped<IScoringStrategy, MultipleChoiceScoringStrategy>();
        services.AddScoped<IScoringStrategy, PsychometricScoringStrategy>();
        services.AddScoped<IScoringService, ScoringService>();
        
        return services;
    }
}