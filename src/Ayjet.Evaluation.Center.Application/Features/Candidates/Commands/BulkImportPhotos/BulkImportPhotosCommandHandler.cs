using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression; // ZipArchive için
using System.Text; // Normalizasyon için

namespace Ayjet.Evaluation.Center.Application.Features.Candidates.Commands.BulkImportPhotos;

public class BulkImportPhotosCommandHandler : IRequestHandler<BulkImportPhotosCommand, BulkImportPhotosResultDto>
{
    private readonly ICandidateRepository _candidateRepository;
    private readonly IFileStorageService _fileStorageService; // Mevcut dosya kayıt servisimiz
    private readonly IUnitOfWork _unitOfWork;

    public BulkImportPhotosCommandHandler(
        ICandidateRepository candidateRepository,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork)
    {
        _candidateRepository = candidateRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
    }

    public async Task<BulkImportPhotosResultDto> Handle(BulkImportPhotosCommand request, CancellationToken cancellationToken)
    {
        var matchFailures = new List<string>();
        int totalFilesFound = 0;
        int successfullyMatched = 0;

        // Hızlı eşleştirme için tüm adayların Ad, Soyad ve ID'lerini hafızaya çekelim
        // İsimleri normalleştirerek bir dictionary'ye atalım
        var allCandidates = await _candidateRepository.GetAsQueryable()
            .Select(c => new { c.Id, c.FirstName, c.LastName })
            .ToListAsync(cancellationToken);

        // "adi_soyadi" -> CandidateId eşleştirmesi için lookup
        var candidateLookup = allCandidates.ToDictionary(
            c => NormalizeName($"{c.FirstName} {c.LastName}"), // "adi soyadi" formatını normalleştir
            c => c.Id
        );

        using var stream = request.ZipFile.OpenReadStream();
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        foreach (var entry in archive.Entries)
        {
            // Sadece jpeg/jpg dosyalarını işle ve __MACOSX gibi metadata klasörlerini atla
            if (entry.FullName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) || 
                entry.FullName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) &&
                !entry.FullName.StartsWith("__"))
            {
                totalFilesFound++;

                // Dosya adını al (örn: "Ahmet_Yılmaz.jpeg")
                var fileName = Path.GetFileNameWithoutExtension(entry.FullName);
                // Dosya adını normalleştir (örn: "ahmet yilmaz")
                var normalizedFileName = NormalizeName(fileName.Replace("_", " "));

                // Eşleşme ara
                if (candidateLookup.TryGetValue(normalizedFileName, out var candidateId))
                {
                    // Adayı tam olarak bulduk, şimdi dosyayı kaydet
                    await using var photoStream = entry.Open();
                    var fileUrl = await _fileStorageService.SaveFileAsync(photoStream, entry.Name); // entry.Name orijinal dosya adıdır

                    // Adayı güncelle
                    var candidate = await _candidateRepository.GetByIdAsync(candidateId, cancellationToken);
                    if (candidate != null)
                    {
                        candidate.ProfilePictureUrl = fileUrl;
                        _candidateRepository.Update(candidate);
                        successfullyMatched++;
                    }
                }
                else
                {
                    matchFailures.Add(entry.FullName);
                }
            }
        }

        if (successfullyMatched > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new BulkImportPhotosResultDto(totalFilesFound, successfullyMatched, matchFailures);
    }

    // İsim eşleştirmesi için Türkçe karakterleri normalleştiren yardımcı metot
    private string NormalizeName(string name)
    {
        if (string.IsNullOrEmpty(name)) return string.Empty;

        var normalized = name.ToLowerInvariant()
            .Replace("ı", "i")
            .Replace("ö", "o")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ç", "c")
            .Replace("ğ", "g");

        // Sadece harf, rakam ve boşluk bırak (veya özel karakterleri temizle)
        // Bu kısım ihtiyaca göre daha da geliştirilebilir. Şimdilik basit tutalım.
        return normalized;
    }
}