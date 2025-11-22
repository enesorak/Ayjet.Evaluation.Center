using Ayjet.Evaluation.Center.Application.Common.Models;
using Ayjet.Evaluation.Center.Application.Features.CandidateAnswers.Commands.Submit;
using Ayjet.Evaluation.Center.Application.Features.TestAssignments.Commands.Cancel;
using Ayjet.Evaluation.Center.Application.Features.TestAssignments.Commands.Create;
using Ayjet.Evaluation.Center.Application.Features.TestAssignments.Commands.Delete;
using Ayjet.Evaluation.Center.Application.Features.TestAssignments.Commands.Finish;
using Ayjet.Evaluation.Center.Application.Features.TestAssignments.Commands.ImportMmpi;
using Ayjet.Evaluation.Center.Application.Features.TestAssignments.Commands.ResendInvitation;
using Ayjet.Evaluation.Center.Application.Features.TestAssignments.Commands.Start;
using Ayjet.Evaluation.Center.Application.Features.TestAssignments.Queries.GetById;
using Ayjet.Evaluation.Center.Application.Features.TestAssignments.Queries.GetList;
using Ayjet.Evaluation.Center.Application.Features.TestAssignments.Queries.GetTestProgress;
using Ayjet.Evaluation.Center.Application.Features.TestAssignments.Queries.GetTestStartInfo;
using Ayjet.Evaluation.Center.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ayjet.Evaluation.Center.Api.Controllers;

[ApiController]
[Route("api/test-assignments")]
public class TestAssignmentsController : ControllerBase
{
    private readonly ISender _mediator;

    public TestAssignmentsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,TestAssigner.MultipleChoice,TestAssigner.Psychometric")]
    public async Task<IActionResult> AssignTest([FromBody] AssignTestCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }



    [HttpGet("{assignmentId}/start-info")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTestStartInfo([FromRoute] string assignmentId)
    {
        var query = new GetTestStartInfoQuery(assignmentId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("{assignmentId}/start")]
    [AllowAnonymous] // Adayın bu işlemi yapmak için login olması gerekmez
    public async Task<IActionResult> StartTest([FromRoute] string assignmentId)
    {
        await _mediator.Send(new StartTestCommand(assignmentId));
        return Ok(new { Message = "Test successfully started." });
    }
    public record SubmitAnswerRequest(int QuestionId, int? SelectedOptionId, int? PsychometricResponse);

    [HttpPost("{assignmentId}/answers")]
    [AllowAnonymous]
    public async Task<IActionResult> SubmitAnswer(
        [FromRoute] string assignmentId, 
        [FromBody] SubmitAnswerRequest requestBody)
    {
        // Komutu, URL'den gelen ID ve Body'den gelen diğer bilgilerle kendimiz oluşturuyoruz.
        var command = new SubmitAnswerCommand(
            assignmentId,
            requestBody.QuestionId,
            requestBody.SelectedOptionId,
            requestBody.PsychometricResponse
        );

        await _mediator.Send(command);
        return Ok();
    }


    [HttpPost("{assignmentId}/finish")]
    [AllowAnonymous] // Adayın bu işlemi yapmak için login olması gerekmez
    public async Task<IActionResult> FinishTest([FromRoute] string assignmentId, [FromBody] FinishTestCommand command)
    {
        // Güvenlik kontrolü: URL'den gelen ID ile BODY'den (JSON) gelen ID'nin
        // aynı olduğundan emin ol.
        if (assignmentId != command.AssignmentId)
        {
            return BadRequest(new { Message = "Assignment ID mismatch between URL and request body." });
        }

        // Artık dolu olan komutu (cevaplar dahil) Handler'a gönderiyoruz.
        await _mediator.Send(command); 
        
        return Ok(new { Message = "Test successfully completed." });
    }


    [HttpGet("{assignmentId}/progress")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTestProgress([FromRoute] string assignmentId)
    {
        var query = new GetTestProgressQuery(assignmentId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }




    [HttpGet]
    [Authorize(Roles = "Admin,TestAssigner.MultipleChoice,TestAssigner.Psychometric")]
    public async Task<IActionResult> GetAssignments(
        [FromQuery] TestAssignmentStatus? status,
        [FromQuery] PaginationParams pageParams) // pageParams parametresini ekleyin
    {
        // Query'yi her iki parametreyle birlikte oluşturun
        return Ok(await _mediator.Send(new GetAssignmentListQuery(status, pageParams)));
    }
    
    [HttpGet("{id}")]
    [AllowAnonymous] // Allow the candidate to fetch this before the test starts
    public async Task<IActionResult> GetById(string id)
    {
        var query = new GetTestAssignmentByIdQuery(id);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
    
    [HttpPost("import-mmpi-csv")]
    [Authorize(Roles = "Admin")]
    // [FromForm] attribute'u IFormFile'dan kaldırıldı, diğerlerinde kaldı.
    public async Task<IActionResult> ImportMmpiAnswersFromCsv(
        [FromForm] string candidateId,
        IFormFile answerFile, // <-- [FromForm] KALDIRILDI
        [FromForm] DateTime? completedAtOverride)
    {
        if (answerFile == null || answerFile.Length == 0)
        {
            return BadRequest("Please upload a CSV file.");
        }
        if (!answerFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Please upload a valid CSV file.");
        }

        var command = new ImportMmpiAnswersCommand(candidateId, answerFile, completedAtOverride);
        var assignmentId = await _mediator.Send(command);
        return Ok(new { message = "MMPI answers imported successfully from CSV.", assignmentId });
    }
    
    
    
    // --- YENİ ENDPOINT: SINAVI SİL (Sadece Pending) ---
    [HttpDelete("{assignmentId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAssignment([FromRoute] string assignmentId)
    {
        await _mediator.Send(new DeleteTestAssignmentCommand(assignmentId));
        return NoContent(); // 204 No Content
    }

    // --- YENİ ENDPOINT: SINAVI İPTAL ET (Durumu Expired'a Çek) ---
    [HttpPost("{assignmentId}/cancel")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CancelAssignment([FromRoute] string assignmentId)
    {
        await _mediator.Send(new CancelTestAssignmentCommand(assignmentId));
        return Ok(new { Message = "Assignment has been cancelled (status set to Expired)." });
    }

    // --- YENİ ENDPOINT: DAVETİYEYİ YENİDEN GÖNDER (ve Süre Uzat) ---
    [HttpPost("{assignmentId}/resend-invitation")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResendInvitation([FromRoute] string assignmentId, [FromBody] ResendInvitationCommand command)
    {
        if (assignmentId != command.AssignmentId)
        {
            return BadRequest("ID mismatch.");
        }
        await _mediator.Send(command);
        return Ok(new { Message = "Test invitation has been resent." });
    }
}
 