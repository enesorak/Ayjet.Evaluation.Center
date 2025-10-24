using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Features.TestResults.Commands.ReScore;
using Ayjet.Evaluation.Center.Application.Features.TestResults.Queries.GetAnswerAnalysis;
using Ayjet.Evaluation.Center.Application.Features.TestResults.Queries.GetByAssignmentId;
using Ayjet.Evaluation.Center.Application.Features.TestResults.Queries.GetPdf;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ayjet.Evaluation.Center.Api.Controllers;

[ApiController]
[Route("api/test-results")]
public class TestResultsController : ControllerBase
{
    private readonly ISender _mediator;

    public TestResultsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{assignmentId}")]
    [Authorize(Roles = "Admin,ReportViewer.MultipleChoice,ReportViewer.Psychometric")]
    public async Task<IActionResult> GetResultForAssignment([FromRoute] string assignmentId)
    {
        var query = new GetTestResultByAssignmentIdQuery(assignmentId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    
    [HttpPost("{assignmentId}/re-score")]
    public async Task<IActionResult> ReScore(string assignmentId)
    {
        await _mediator.Send(new ReScoreTestCommand(assignmentId));
        return Accepted(); // İşin arka plana alındığını belirtmek için 202 Accepted
    }
    
    
    [HttpGet("{assignmentId}/answer-analysis")]
    [Authorize(Roles = "Admin,ReportViewer.MultipleChoice,ReportViewer.Psychometric")]
    public async Task<IActionResult> GetAnswerAnalysis(string assignmentId)
    {
        var query = new GetAnswerAnalysisQuery(assignmentId);
        return Ok(await _mediator.Send(query));
    }
    
    // --- YENİ PDF ENDPOINT'İ ---
    [HttpGet("{assignmentId}/pdf")]
    //[Authorize(Roles = "Admin,ReportViewer.Psychometric")] // PDF şimdilik sadece MMPI için
    public async Task<IActionResult> GetTestResultPdf([FromRoute] string assignmentId)
    {
        var query = new GetTestResultPdfQuery(assignmentId);
        try
        {
            byte[] pdfBytes = await _mediator.Send(query);

            // Dinamik bir dosya adı oluşturabiliriz (Opsiyonel)
            // Önce adayın adını vb. çekmek gerekebilir, şimdilik sabit
            var fileName = $"MMPI_Rapor_{assignmentId}.pdf";

            // PDF dosyasını FileResult olarak döndür
            return File(pdfBytes, "application/pdf", fileName);
        }
        
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (NotSupportedException ex)
        {
            return BadRequest(ex.Message); // Desteklenmeyen test tipi için 400
        }
        catch (Exception ex)
        {
            // Beklenmedik hatalar için 500
            // Loglama mekanizması varsa burada loglama yapılmalı
            return StatusCode(500, $"An unexpected error occurred while generating the PDF: {ex.Message}");
        }
    }
    // ----------------------------
}