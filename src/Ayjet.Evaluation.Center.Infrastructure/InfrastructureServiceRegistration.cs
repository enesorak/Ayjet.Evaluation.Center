using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Infrastructure.Email;
using Ayjet.Evaluation.Center.Infrastructure.Notifications;
using Ayjet.Evaluation.Center.Infrastructure.Reporting;
using Ayjet.Evaluation.Center.Infrastructure.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ayjet.Evaluation.Center.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IEmailService, EmailService>();
        
        
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options =>
                options.UseNpgsqlConnection(configuration.GetConnectionString("PostgreSQL"))
            ));

        services.AddHangfireServer();
        
        
        services.AddSingleton<IFileStorageService, LocalStorageService>();
        
        services.AddSingleton<INotificationService, SignalRNotificationService>();
        
        
        services.AddScoped<IPdfReportEngine, PdfReportEngine>(); // <-- YENİ SATIR
        return services;
    }
}