using System.Text;
using System.Text.Json.Serialization;
using Ayjet.Evaluation.Center.Persistence;
using Ayjet.Evaluation.Center.Api.Filters;
using Ayjet.Evaluation.Center.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Ayjet.Evaluation.Center.Application;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Infrastructure;
using Ayjet.Evaluation.Center.Infrastructure.Hubs;
using Ayjet.Evaluation.Center.Persistence.Context;
using Ayjet.Evaluation.Center.Persistence.DataSeeders;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/api/notificationHub")))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!))
        };
    });

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policyBuilder =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policyBuilder.WithOrigins(
                    "http://localhost:5173",
                    "http://localhost:3000",
                    "http://localhost:8080"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithExposedHeaders("Content-Disposition");
        }
        else
        {
            policyBuilder.WithOrigins(
                    "https://evaluation.ayjet.aero",
                    "http://evaluation.ayjet.aero",
                    "http://localhost:5000",
                    "http://localhost:8088"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithExposedHeaders("Content-Disposition")
                .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
        }
    });
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

QuestPDF.Settings.License = LicenseType.Community;
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ayjet.Evaluation.Center.Api", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer **\""
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// =========================================
// DATABASE MIGRATION & SEEDING
// =========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var env = services.GetRequiredService<IWebHostEnvironment>();
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        
        if (env.IsDevelopment())
        {
            logger.LogInformation("🔧 Development mode: Running automatic migrations...");
            
            // Development: Otomatik migration + seed
            dbContext.Database.Migrate();
            await IdentityDataSeeder.SeedRolesAndAdminAsync(services);
            await TestContentSeeder.SeedMmpiTestAsync(dbContext);
            
            logger.LogInformation("✅ Migrations and seeding completed successfully.");
        }
        else // Production
        {
            logger.LogInformation("🏭 Production mode: Checking for pending migrations...");
            
            // Production: Sadece seed data (migration manuel)
            await IdentityDataSeeder.SeedRolesAndAdminAsync(services);
            
            // Pending migration kontrolü ve uyarı
            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                logger.LogWarning(
                    "⚠️ WARNING: {Count} pending migration(s) detected: {Migrations}",
                    pendingMigrations.Count(),
                    string.Join(", ", pendingMigrations)
                );
                logger.LogWarning(
                    "⚠️ Please run migrations manually: dotnet ef database update"
                );
            }
            else
            {
                logger.LogInformation("✅ Database is up to date.");
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ An error occurred during database initialization.");
        
        // Production'da hata durumunda uygulamayı başlatma
        if (!env.IsDevelopment())
        {
            logger.LogCritical("❌ CRITICAL: Cannot start application due to database error.");
            throw; // Uygulamayı durdur
        }
    }
}

// =========================================
// MIDDLEWARE PIPELINE
// =========================================
app.UseMiddleware<ExceptionHandlerMiddleware>();

var swaggerEnabled = builder.Configuration.GetValue<bool>("SwaggerEnabled");

if (swaggerEnabled)
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.RoutePrefix = "swagger";
        c.SwaggerEndpoint("v1/swagger.json", "Ayjet Evaluation API v1");
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowVueApp");

// Authentication/Authorization
app.UseAuthentication();
app.UseAuthorization();

// Hangfire Dashboard (Admin only)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireDashboardAuthorizationFilter() }
});

app.MapControllers();
app.MapHub<NotificationHub>("/api/notificationHub");

app.MapGet("/debug/routes", (IEnumerable<EndpointDataSource> endpointSources) =>
{
    var endpoints = new List<string>();
    foreach (var source in endpointSources)
    {
        endpoints.AddRange(source.Endpoints.OfType<RouteEndpoint>().Select(e =>
            $"[{e.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.FirstOrDefault()}] {e.RoutePattern.RawText}"
        ));
    }
    return Results.Ok(endpoints.OrderBy(r => r));
});

app.Run();