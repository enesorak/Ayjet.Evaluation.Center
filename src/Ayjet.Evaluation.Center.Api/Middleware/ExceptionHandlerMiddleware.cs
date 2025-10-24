using System.Net;
using System.Text.Json;
using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Ayjet.Evaluation.Center.Api.Middleware;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception has occurred: {Message}", ex.Message);

            // Veritabanı güncelleme hatası mı ve içinde unique constraint ihlali var mı diye kontrol et.
            if (ex is DbUpdateException dbUpdateEx && dbUpdateEx.InnerException is PostgresException postgresEx && postgresEx.SqlState == "23505")
            {
                // Eğer öyleyse, bunu bizim ConflictException'ımıza çevir.
                // Constraint adından hangi alanın sorunlu olduğunu anlayabiliriz.
                // ...
                var errorMessage = postgresEx.ConstraintName switch
                {
                    "IX_Candidates_InitialCode" => "This Initial Code has already been used by another candidate.",
                    "IX_Candidates_Email" => "A candidate with this email address is already registered.",
                    _ => "One of the provided values conflicts with an existing resource."
                };
// ...

                await HandleExceptionAsync(context, new ConflictException(errorMessage));
            }
            else
            {
                // Diğer tüm hatalar için mevcut mantığı kullan.
                await HandleExceptionAsync(context, ex);
            }
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = context.Response;

        var errorResponse = new
        {
            Success = false,
            Message = "An internal server error has occurred."
        };

        switch (exception)
        {
            case NotFoundException ex:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse = new { Success = false, Message = ex.Message };
                break;
            case ConflictException ex:
                response.StatusCode = (int)HttpStatusCode.Conflict; // 409 Conflict
                errorResponse = new { Success = false, Message = ex.Message };
                break;
            // case ValidationException ex:
            //     response.StatusCode = (int)HttpStatusCode.BadRequest;
            //     errorResponse = new { Success = false, Message = "Validation failed.", Errors = ex.Errors };
            //     break;
            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        var result = JsonSerializer.Serialize(errorResponse);
        return context.Response.WriteAsync(result);
    }
}