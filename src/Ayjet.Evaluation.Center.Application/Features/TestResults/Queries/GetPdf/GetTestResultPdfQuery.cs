using MediatR;

// Namespace'i kendi projenize göre ayarlayın
namespace Ayjet.Evaluation.Center.Application.Features.TestResults.Queries.GetPdf;

// Bu query işlendiğinde geriye PDF dosyasının byte dizisini dönecek
public record GetTestResultPdfQuery(string AssignmentId) : IRequest<byte[]>;