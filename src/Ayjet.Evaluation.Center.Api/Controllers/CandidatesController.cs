using System.Security.Claims;
using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Application.Common.Models;
using Ayjet.Evaluation.Center.Application.Features.Candidates.Commands.BulkImport;
using Ayjet.Evaluation.Center.Application.Features.Candidates.Commands.BulkImportPhotos;
using Ayjet.Evaluation.Center.Application.Features.Candidates.Commands.ConfirmProfile;
using Ayjet.Evaluation.Center.Application.Features.Candidates.Commands.Create;
using Ayjet.Evaluation.Center.Application.Features.Candidates.Commands.Delete;
using Ayjet.Evaluation.Center.Application.Features.Candidates.Commands.SetArchiveStatus;
using Ayjet.Evaluation.Center.Application.Features.Candidates.Commands.UpdateProfile;
using Ayjet.Evaluation.Center.Application.Features.Candidates.Commands.UpdateProfilePicture;
using Ayjet.Evaluation.Center.Application.Features.Candidates.DTOs;
using Ayjet.Evaluation.Center.Application.Features.Candidates.Queries.GetById;
using Ayjet.Evaluation.Center.Application.Features.Candidates.Queries.GetCounts;
using Ayjet.Evaluation.Center.Application.Features.Candidates.Queries.GetList;
using Ayjet.Evaluation.Center.Application.Features.TestAssignments.Queries.GetListByCandidate;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Enums;
using ClosedXML.Excel;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ayjet.Evaluation.Center.Api.Controllers;

[ApiController]
[Route("api/candidates")]
[Authorize(Roles = "Admin")]
public class CandidatesController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly ICandidateRepository _candidateRepo;
    public CandidatesController(ISender mediator, ICandidateRepository candidateRepo)
    {
        _mediator = mediator;
        _candidateRepo = candidateRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] PaginationParams pageParams,
        [FromQuery] string? searchTerm,
        [FromQuery] Department? department,
        [FromQuery] bool? isArchived,
        [FromQuery] TestAssignmentStatus? assignmentStatus) // <-- Yeni parametre
    {
        var query = new GetCandidateListQuery(pageParams, searchTerm, department, isArchived, assignmentStatus);
        return Ok(await _mediator.Send(query));
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCandidateCommand command)
    {
        var candidateId = await _mediator.Send(command);
        return Ok(new { id = candidateId });
    }
    
    
    [HttpPost("{id}/archive-status")]
    public async Task<IActionResult> SetArchiveStatus(string id, [FromBody] SetCandidateArchiveStatusCommand command)
    {
        // Gelen komuttaki ID ile URL'deki ID'nin aynı olduğunu kontrol edelim (ekstra güvenlik)
        if (id != command.CandidateId)
        {
            return BadRequest("ID mismatch between route and body.");
        }
        
        await _mediator.Send(command);
        return NoContent();
    }

  
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        await _mediator.Send(new DeleteCandidateCommand(id));
        return NoContent(); // Başarılı silme işlemi sonrası 204 No Content
    }
    
    [HttpGet("counts")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetCounts()
    {
        return Ok(await _mediator.Send(new GetCandidateCountsQuery()));
    }
    [HttpPost("{id}/confirm-profile")]
    [AllowAnonymous] // Adayın erişebilmesi için
    public async Task<IActionResult> ConfirmProfile(string id)
    {
        await _mediator.Send(new ConfirmCandidateProfileCommand(id));
        return NoContent();
    }
    
    [HttpGet("{id}")]
    [AllowAnonymous]

    public async Task<IActionResult> GetById(string id)
    {
        // Bu işlem için ayrı bir Query/Handler oluşturmak en doğrusu,
        // ama hızlanmak için şimdilik repository'yi doğrudan kullanabiliriz.
        var candidate = await _mediator.Send(new GetCandidateByIdQuery(id)); // GetCandidateByIdQuery oluşturulmalı
        return Ok(candidate);
    }
    
    
    [HttpGet("{id}/assignments")]
    public async Task<IActionResult> GetAssignmentsForCandidate(string id)
    {
        return Ok(await _mediator.Send(new GetAssignmentsByCandidateQuery(id)));
    }
    
    
    [HttpGet("{id}/profile")]
    public async Task<IActionResult> GetProfile(string id)
    {
        var candidate = await _candidateRepo.GetByIdAsync(id)
                        ?? throw new NotFoundException(nameof(Candidate), id);
        
        // Güvenlik kontrolü: Sadece admin veya adayın kendisi profili görebilir
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        // Bu mantığı daha da geliştirebiliriz. Şimdilik admin kontrolü yeterli.
        if (!User.IsInRole("Admin"))
        {
            // Adayın kendi email'i ile eşleşme kontrolü vs. yapılabilir.
            // Şimdilik basitleştiriyoruz.
        }

        var profileDto = new CandidateProfileDto
        {
            Gender = candidate.Gender,
            MaritalStatus = candidate.MaritalStatus,
            Profession = candidate.Profession,
            EducationLevel = candidate.EducationLevel
        };
        return Ok(profileDto);
    }

  
    
    [HttpPost("{id}/profile-picture")]
    [AllowAnonymous]
     
    public async Task<IActionResult> UpdateProfilePicture(string id, IFormFile profilePicture) // <-- Parameter name changed to 'profilePicture'
    {
        if (profilePicture == null || profilePicture.Length == 0)
        {
            return BadRequest("Lütfen bir dosya seçin.");
        }

        var command = new UpdateCandidateProfilePictureCommand(id, profilePicture);
        var url = await _mediator.Send(command);

        return Ok(new { profilePictureUrl = url });
    }
    
    [HttpPut("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateCandidateProfileCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID uyuşmazlığı.");
        }
        await _mediator.Send(command);
        return NoContent();
    }
    
    
    [HttpPut("{id}/profile")]
    [AllowAnonymous]
    //[Authorize]
    public async Task<IActionResult> UpdateProfile(string id, [FromBody] UpdateProfileDto dto)
    {
        // Construct the full command using the ID from the URL and the data from the body
        var command = new UpdateCandidateProfileCommand(
            id,
            dto.FirstName,
            dto.LastName,
            dto.Email,
            dto.InitialCode,
            dto.FleetCode,
            //dto.CandidateType, // <-- Eklendi
            //to.Department,
          
            dto.Gender, 
            dto.BirthDate,
            dto.MaritalStatus,
            dto.Profession,
            dto.EducationLevel
        );

        await _mediator.Send(command);
        return NoContent();
    }
    
    
    // --- YENİ TOPLU ADAY YÜKLEME ENDPOINT'İ ---
    [HttpPost("bulk-import-candidates")]
    [Consumes("multipart/form-data")] // Form-data olarak dosya alacağımızı belirtir
    public async Task<IActionResult> BulkImportCandidates(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Please upload a valid Excel file.");
        }
        if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Only .xlsx files are supported.");
        }

        var command = new BulkImportCandidatesCommand(file);
        var result = await _mediator.Send(command);

        if (result.FailedCount > 0)
        {
            // Kısmi başarı (bazıları eklendi, bazıları eklenemedi)
            return Ok(result); 
        }

        // Tam başarı
        return Ok(result);
    }
    
    
    
    // --- YENİ TOPLU FOTOĞRAF YÜKLEME ENDPOINT'İ ---
    [HttpPost("bulk-import-photos")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> BulkImportPhotos(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Please upload a valid ZIP file.");
        }
        if (!Path.GetExtension(file.FileName).Equals(".zip", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Only .zip files are supported.");
        }

        var command = new BulkImportPhotosCommand(file);
        var result = await _mediator.Send(command);

        return Ok(result); // Başarılı/başarısız eşleşme sayısını döndür
    }
    
    // --- YENİ ŞABLON İNDİRME ENDPOINT'İ ---
    [HttpGet("bulk-import-template")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetCandidateImportTemplate()
    {
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Adaylar");

            // Başlıkları oluştur (senin istediğin alanlar)
            var headers = new string[]
            {
                "Ad",         // Adı
                "Soyad",      // Soyadi
                "Filo",       // Gruplar -> FleetCode
                "InitialId",  // Kısa Kod -> InitialCode
                "DogumTarihi",// Dogum Tarihi
                "Telefon",    // GSM
                "Email"       // Email
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            }
            
            // Tarih sütunu için formatlama ve örnek ekleyelim
            var dateCell = worksheet.Cell(2, 5);
            dateCell.Value = "1999-10-25"; // Örnek tarih
            dateCell.Style.NumberFormat.Format = "yyyy-mm-dd"; // Formatı belirt
            worksheet.Column(5).Style.NumberFormat.Format = "yyyy-mm-dd"; // Tüm sütuna uygula
            
            // Örnek bir satır ekleyelim ki kullanıcı formatı görsün
            worksheet.Cell(2, 1).Value = "Ahmet";
            worksheet.Cell(2, 2).Value = "Yılmaz";
            worksheet.Cell(2, 3).Value = "147";
            worksheet.Cell(2, 4).Value = "147AY";
            worksheet.Cell(2, 6).Value = "05551234567";
            worksheet.Cell(2, 7).Value = "ahmet.yilmaz@ornek.com";

            // Sütun genişliklerini ayarla
            worksheet.Columns().AdjustToContents();

            // Dosyayı bir MemoryStream'e kaydet
            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                var content = stream.ToArray();
                var mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                var fileName = "AdayYuklemeSablonu.xlsx";

                // Dosyayı tarayıcıya gönder
                return File(content, mimeType, fileName);
            }
        }
        
        
    }
    
}