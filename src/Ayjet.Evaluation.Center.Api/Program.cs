using System.Text;
using System.Text.Json.Serialization;
using AAyjet.Evaluation.Center.Persistence;
using AAyjet.Evaluation.Center.Persistence.Context;
using AAyjet.Evaluation.Center.Persistence.DataSeeders;
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
    });


builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policyBuilder =>
    {
        // Vue geliştirme sunucunuzun adresini buraya yazın.
        // Port numarası farklıysa güncelleyin.
        policyBuilder.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials() // <-- BU SATIR ÇOK ÖNEMLİ
            .WithExposedHeaders("Content-Disposition"); // <-- BU SATIRI EKLEYİN
        ;
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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();


app.UseRouting();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireDashboardAuthorizationFilter()]
});

app.UseCors("AllowVueApp");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapHub<NotificationHub>("/notificationHub");

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