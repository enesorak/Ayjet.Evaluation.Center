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
using Ayjet.Evaluation.Center.Persistence;
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
                    (path.StartsWithSegments("/notificationHub")))
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
            IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!))
        };
        
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // SignalR token'ını query string'den oku
                var accessToken = context.Request.Query["access_token"];

                // Path'in hub endpoint'imize ait olup olmadığını kontrol et
                // (Frontend /service/api/notificationHub'ı aradığı için,
                // IIS'in /service/ kısmını attıktan sonra backend'e /api/notificationHub gelir)
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/api/notificationHub"))) // Bizim hub'ın yolu
                {
                    // Token'ı context'e ata, böylece [Authorize] attribute'u onu görür
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policyBuilder =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Development için localhost
            policyBuilder.WithOrigins(
                    "http://localhost:5173",
                    "http://localhost:3000",
                    "http://localhost:8080"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithExposedHeaders("Content-Disposition"); // Dosya indirme için gerekli
        }
        else
        {
            // Production için domain
            policyBuilder.WithOrigins(
                    "https://evaluation.ayjet.aero",
                    "http://evaluation.ayjet.aero" ,
                    "http://localhost:5000",
                    "http://localhost:8088"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithExposedHeaders("Content-Disposition") // Dosya indirme için gerekli
                .SetPreflightMaxAge(TimeSpan.FromMinutes(10)); // Preflight cache süresi
        }
    });
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Enum'ları sayı yerine metin olarak serialize et (örn: 2 yerine "Psychometric")
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
        Description =
            "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer **\""
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
            []
        }
    });
});

var app = builder.Build();

 

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        // Eğer bekleyen migration varsa, uygula (veritabanı yoksa oluşturur).
        dbContext.Database.Migrate();

        // Veri tohumlama işlemini de burada yapalım.
        await IdentityDataSeeder.SeedRolesAndAdminAsync(services);
        await TestContentSeeder.SeedMmpiTestAsync(dbContext);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}


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
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireDashboardAuthorizationFilter()]
});



app.UseAuthentication();
app.UseAuthorization();
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