using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Enums;
using ClosedXML.Excel; // Excel okumak için
using MediatR;
using Microsoft.EntityFrameworkCore; // ToListAsync için

namespace Ayjet.Evaluation.Center.Application.Features.Candidates.Commands.BulkImport;

public class BulkImportCandidatesCommandHandler : IRequestHandler<BulkImportCandidatesCommand, BulkImportResultDto>
{
    private readonly ICandidateRepository _candidateRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BulkImportCandidatesCommandHandler(ICandidateRepository candidateRepository, IUnitOfWork unitOfWork)
    {
        _candidateRepository = candidateRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BulkImportResultDto> Handle(BulkImportCandidatesCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var newCandidates = new List<Candidate>();
        int successCount = 0;

        // Hızlı e-posta kontrolü için mevcut tüm e-postaları bir HashSet'e çekelim.
        var existingEmails = (await _candidateRepository.GetAsQueryable()
            .Select(c => c.Email.ToLower())
            .ToListAsync(cancellationToken))
            .ToHashSet();

        using var stream = request.ExcelFile.OpenReadStream();
        using var workbook = new XLWorkbook(stream);

        var worksheet = workbook.Worksheets.FirstOrDefault();
        if (worksheet == null || worksheet.RowsUsed().Count() <= 1)
        {
            return new BulkImportResultDto(0, 0, new List<string> { "Excel file is empty or invalid." });
        }

        // İlk satır başlık, o yüzden atla
        var rows = worksheet.RowsUsed().Skip(1); 

        foreach (var row in rows)
        {
            int rowNumber = row.RowNumber();
            try
            {
                // Başlıklara göre değil, sütun sırasına göre okuyoruz:
                // Adı, Soyadi, Filo, Initial ID, Dogum Tarihi, Telefon, Email
                var email = row.Cell(7).GetValue<string>()?.Trim();

                // E-posta zorunlu mu? Evet.
                if (string.IsNullOrWhiteSpace(email))
                {
                    errors.Add($"Row {rowNumber}: Email field is required.");
                    continue;
                }

                // E-posta sistemde veya bu excelde daha önce eklendi mi?
                if (existingEmails.Contains(email.ToLower()) || newCandidates.Any(c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
                {
                    errors.Add($"Row {rowNumber}: Email '{email}' already exists.");
                    continue;
                }

                var candidate = new Candidate
                {
                    FirstName = row.Cell(1).GetValue<string>().Trim(),
                    LastName = row.Cell(2).GetValue<string>().Trim(),
                    FleetCode = row.Cell(3).GetValue<string>()?.Trim(),
                    InitialCode = row.Cell(4).GetValue<string>()?.Trim(),
                    BirthDate = row.Cell(5).TryGetValue(out DateOnly date) ? date : null,
                    PhoneNumber = row.Cell(6).GetValue<string>()?.Trim(),
                    Email = email,
                    // Bu alanlar Excel'de olmadığı için varsayılan değer atıyoruz
                    Department = Department.Unspecified,
                    CandidateType = CandidateType.Student,
                    CreatedAt = DateTime.UtcNow
                };

                newCandidates.Add(candidate);
            }
            catch (Exception ex)
            {
                errors.Add($"Row {rowNumber}: An unexpected error occurred. {ex.Message}");
            }
        }

        if (newCandidates.Any())
        {
            // Toplu ekleme
            await _candidateRepository.AddRangeAsync(newCandidates, cancellationToken);
            // Tek seferde kaydetme
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            successCount = newCandidates.Count;
        }

        return new BulkImportResultDto(successCount, errors.Count, errors);
    }
}