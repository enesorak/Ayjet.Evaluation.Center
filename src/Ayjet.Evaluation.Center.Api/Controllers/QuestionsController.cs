using Ayjet.Evaluation.Center.Application.Features.MultipleChoiceQuestions.Commands.Delete;
using Ayjet.Evaluation.Center.Application.Features.MultipleChoiceQuestions.Commands.Update;
using Ayjet.Evaluation.Center.Application.Features.PsychometricQuestions.Commands.Update;
using Ayjet.Evaluation.Center.Application.Features.PsychometricQuestions.Queries.GetById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ayjet.Evaluation.Center.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] // Bu controller'a sadece Admin erişebilir
public class QuestionsController : ControllerBase
{
    private readonly ISender _mediator;
    public QuestionsController(ISender mediator) => _mediator = mediator;

    [HttpDelete("{id:int}")] // Sadece integer ID kabul et
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        await _mediator.Send(new DeleteQuestionCommand(id));
        return NoContent();
    }
    
    
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateQuestionCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        await _mediator.Send(command);
        return NoContent();
    }
    
    
    
    [HttpGet("psychometric/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPsychometricQuestionDetail(int id)
    {
        var query = new GetPsychometricQuestionDetailQuery(id);
        return Ok(await _mediator.Send(query));
    }
    
    
    [HttpPut("psychometric/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdatePsychometricQuestion(int id, [FromBody] UpdatePsychometricQuestionCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID mismatch between route and body.");
        }
        await _mediator.Send(command);
        return NoContent(); // Başarılı güncelleme için 204 No Content
    }
}