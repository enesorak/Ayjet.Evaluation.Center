using MediatR;
using Microsoft.AspNetCore.Http;

namespace Ayjet.Evaluation.Center.Application.Features.MultipleChoiceQuestions.Commands.BulkImport;

// BulkImportQuestionsCommand.cs

// IFormFile için

// Bu komut, bir IFormFile alacak ve bir sonuç özeti dönecek
public record BulkImportQuestionsCommand(
    string TestDefinitionId, 
    IFormFile ExcelFile) : IRequest<ImportResultDto>;

// İşlem sonucunu döndürecek olan DTO