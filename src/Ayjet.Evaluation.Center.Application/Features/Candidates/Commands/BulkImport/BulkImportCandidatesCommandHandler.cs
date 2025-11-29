using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Enums;
using ClosedXML.Excel;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
        var candidatesToAdd = new List<Candidate>();

        // 1. Mevcut verileri DB'den çek (Email ve InitialCode)
        var existingEmails = (await _candidateRepository.GetAsQueryable()
            .Select(c => c.Email.ToLower())
            .ToListAsync(cancellationToken))
            .ToHashSet();

        var existingInitialCodes = (await _candidateRepository.GetAsQueryable()
            .Where(c => c.InitialCode != null)
            .Select(c => c.InitialCode!.ToLower())
            .ToListAsync(cancellationToken))
            .ToHashSet();

        // 2. Excel'i oku
        using var stream = request.ExcelFile.OpenReadStream();
        using var workbook = new XLWorkbook(stream);

        var worksheet = workbook.Worksheets.FirstOrDefault();
        if (worksheet == null || worksheet.RowsUsed().Count() <= 1)
        {
            return new BulkImportResultDto(0, 0, new List<string> { "Excel dosyası boş veya geçersiz." });
        }

        var rows = worksheet.RowsUsed().Skip(1).ToList();

        // 3. İlk geçiş: Excel'deki tüm InitialCode'ları topla ve duplicate kontrolü yap
        var excelInitialCodes = new Dictionary<string, List<int>>(); // code -> satır numaraları
        var excelEmails = new Dictionary<string, List<int>>(); // email -> satır numaraları

        foreach (var row in rows)
        {
            int rowNumber = row.RowNumber();
            
            var initialCode = row.Cell(4).GetValue<string>()?.Trim();
            var email = row.Cell(7).GetValue<string>()?.Trim();

            // InitialCode varsa kaydet
            if (!string.IsNullOrWhiteSpace(initialCode))
            {
                var codeKey = initialCode.ToLower();
                if (!excelInitialCodes.ContainsKey(codeKey))
                    excelInitialCodes[codeKey] = new List<int>();
                excelInitialCodes[codeKey].Add(rowNumber);
            }

            // Email varsa kaydet
            if (!string.IsNullOrWhiteSpace(email))
            {
                var emailKey = email.ToLower();
                if (!excelEmails.ContainsKey(emailKey))
                    excelEmails[emailKey] = new List<int>();
                excelEmails[emailKey].Add(rowNumber);
            }
        }

        // 4. Excel içi duplicate kontrolü
        foreach (var kvp in excelInitialCodes.Where(x => x.Value.Count > 1))
        {
            errors.Add($"InitialCode '{kvp.Key.ToUpper()}' Excel içinde tekrarlanıyor (satırlar: {string.Join(", ", kvp.Value)})");
        }

        foreach (var kvp in excelEmails.Where(x => x.Value.Count > 1))
        {
            errors.Add($"Email '{kvp.Key}' Excel içinde tekrarlanıyor (satırlar: {string.Join(", ", kvp.Value)})");
        }

        // 5. DB ile çakışma kontrolü
        foreach (var kvp in excelInitialCodes)
        {
            if (existingInitialCodes.Contains(kvp.Key))
            {
                errors.Add($"InitialCode '{kvp.Key.ToUpper()}' veritabanında zaten mevcut (satır: {kvp.Value.First()})");
            }
        }

        foreach (var kvp in excelEmails)
        {
            if (existingEmails.Contains(kvp.Key))
            {
                errors.Add($"Email '{kvp.Key}' veritabanında zaten mevcut (satır: {kvp.Value.First()})");
            }
        }

        // 6. Eğer herhangi bir duplicate varsa, hiçbir şey kaydetme
        if (errors.Any())
        {
            return new BulkImportResultDto(0, rows.Count, errors);
        }

        // 7. İkinci geçiş: Candidate nesnelerini oluştur
        foreach (var row in rows)
        {
            int rowNumber = row.RowNumber();
            try
            {
                var email = row.Cell(7).GetValue<string>()?.Trim();

                if (string.IsNullOrWhiteSpace(email))
                {
                    errors.Add($"Satır {rowNumber}: Email alanı zorunludur.");
                    continue;
                }

                // Doğum tarihi parsing - DateTime olarak oku, DateOnly'ye çevir
                DateOnly? birthDate = null;
                var birthDateCell = row.Cell(5);
                if (!birthDateCell.IsEmpty())
                {
                    if (birthDateCell.TryGetValue(out DateTime dateTime))
                    {
                        birthDate = DateOnly.FromDateTime(dateTime);
                    }
                    else
                    {
                        // String olarak dene
                        var dateStr = birthDateCell.GetValue<string>()?.Trim();
                        if (!string.IsNullOrWhiteSpace(dateStr) && DateTime.TryParse(dateStr, out var parsedDate))
                        {
                            birthDate = DateOnly.FromDateTime(parsedDate);
                        }
                    }
                }

                var candidate = new Candidate
                {
                    FirstName = row.Cell(1).GetValue<string>()?.Trim() ?? "",
                    LastName = row.Cell(2).GetValue<string>()?.Trim() ?? "",
                    FleetCode = NullIfEmpty(row.Cell(3).GetValue<string>()?.Trim()),
                    InitialCode = NullIfEmpty(row.Cell(4).GetValue<string>()?.Trim()),
                    BirthDate = birthDate,
                    PhoneNumber = NullIfEmpty(row.Cell(6).GetValue<string>()?.Trim()),
                    Email = email,
                    Department = Department.Unspecified,
                    CandidateType = CandidateType.Student,
                    CreatedAt = DateTime.UtcNow
                };

                candidatesToAdd.Add(candidate);
            }
            catch (Exception ex)
            {
                errors.Add($"Satır {rowNumber}: Beklenmeyen bir hata oluştu. {ex.Message}");
            }
        }

        // 8. Eğer parsing sırasında hata olduysa, yine kaydetme
        if (errors.Any())
        {
            return new BulkImportResultDto(0, rows.Count, errors);
        }

        // 9. Toplu kaydet
        if (candidatesToAdd.Any())
        {
            await _candidateRepository.AddRangeAsync(candidatesToAdd, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new BulkImportResultDto(candidatesToAdd.Count, 0, errors);
    }

    private static string? NullIfEmpty(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}