using Ayjet.Evaluation.Center.Application.Common.Models;
using Ayjet.Evaluation.Center.Application.Features.MultipleChoiceQuestions.Commands.BulkImport;
using Ayjet.Evaluation.Center.Application.Features.MultipleChoiceQuestions.Commands.Create;
using Ayjet.Evaluation.Center.Application.Features.TestDefinitions.Commands.Create;
using Ayjet.Evaluation.Center.Application.Features.TestDefinitions.Commands.Delete;
using Ayjet.Evaluation.Center.Application.Features.TestDefinitions.Commands.Update;
using Ayjet.Evaluation.Center.Application.Features.TestDefinitions.Queries.GetList;
using Ayjet.Evaluation.Center.Application.Features.TestDefinitions.Queries.GetQuestionsByTestDefinition;
using ClosedXML.Excel;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ayjet.Evaluation.Center.Api.Controllers;

 [ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] // Bu controller'daki çoğu eylem Admin yetkisi gerektirir
public class TestDefinitionsController : ControllerBase
{
    private readonly ISender _mediator;

    public TestDefinitionsController(ISender mediator)
    {
        _mediator = mediator;
    }

    // === Test Definition CRUD ===

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] PaginationParams pageParams)
    {
        var result = await _mediator.Send(new GetTestDefinitionListQuery(pageParams));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTestDefinitionCommand command)
    {
        var newTestId = await _mediator.Send(command);
        return Ok(new { id = newTestId });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateTestDefinitionCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] string id)
    {
        await _mediator.Send(new DeleteTestDefinitionCommand { Id = id });
        return NoContent();
    }

    // === Question Management for a specific Test Definition ===

    [HttpPost("{testDefinitionId}/questions")]
    public async Task<IActionResult> AddQuestion(
        [FromRoute] string testDefinitionId,
        [FromBody] CreateMultipleChoiceQuestionCommand command)
    {
        var updatedCommand = command with { TestDefinitionId = testDefinitionId };

        var newQuestionId = await _mediator.Send(updatedCommand);

        return Ok(new { id = newQuestionId });
    }

    [HttpGet("{testDefinitionId}/questions")]
    public async Task<IActionResult> GetQuestions(
        [FromRoute] string testDefinitionId,
        [FromQuery] string? searchTerm,
        [FromQuery] PaginationParams pageParams) // <-- Eklendi
    {
        var query = new GetQuestionsByTestDefinitionQuery(testDefinitionId, searchTerm, pageParams);
        return Ok(await _mediator.Send(query));
    }
    
    
    [HttpPost("{testDefinitionId}/questions/bulk-import")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkImportQuestions(string testDefinitionId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Lütfen bir Excel dosyası yükleyin.");
        }

        // Dosya uzantısını kontrol et
        if (Path.GetExtension(file.FileName).ToLower() != ".xlsx")
        {
            return BadRequest("Lütfen sadece .xlsx formatında bir Excel dosyası yükleyin.");
        }

        var command = new BulkImportQuestionsCommand(testDefinitionId, file);
        var result = await _mediator.Send(command);

        if (result.FailedCount > 0)
        {
            // Kısmi başarı durumunda hem başarılı sayısını hem de hataları döndür
            return Ok(result); 
        }

        return Ok(result);
    }
    
    
    [HttpGet("questions/bulk-import-template")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetBulkImportTemplate()
    {
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Sorular");

            // Başlıkları oluştur
            var headers = new string[]
            {
                "SoruMetni", "Dil", "SoruKodu", "SiraNo", "Zorluk",
                "DogruCevapNo", "Secenek1", "Secenek2", "Secenek3", "Secenek4", "Secenek5"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            worksheet.Columns().AdjustToContents();

            // Dosyayı bir MemoryStream'e kaydet
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();
                var mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                var fileName = "SoruYuklemeSablonu.xlsx";

                return File(content, mimeType, fileName);
            }
        }
    }
}