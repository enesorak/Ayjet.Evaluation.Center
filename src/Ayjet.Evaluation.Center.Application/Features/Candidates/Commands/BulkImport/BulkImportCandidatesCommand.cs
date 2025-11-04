using MediatR;
using Microsoft.AspNetCore.Http; // IFormFile için

namespace Ayjet.Evaluation.Center.Application.Features.Candidates.Commands.BulkImport;

// Komut, IFormFile alacak ve bir BulkImportResultDto dönecek
public record BulkImportCandidatesCommand(
    IFormFile ExcelFile
) : IRequest<BulkImportResultDto>;