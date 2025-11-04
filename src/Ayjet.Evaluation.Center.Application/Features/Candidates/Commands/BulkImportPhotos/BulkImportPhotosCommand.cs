using MediatR;
using Microsoft.AspNetCore.Http;

namespace Ayjet.Evaluation.Center.Application.Features.Candidates.Commands.BulkImportPhotos;

public record BulkImportPhotosCommand(
    IFormFile ZipFile
) : IRequest<BulkImportPhotosResultDto>;