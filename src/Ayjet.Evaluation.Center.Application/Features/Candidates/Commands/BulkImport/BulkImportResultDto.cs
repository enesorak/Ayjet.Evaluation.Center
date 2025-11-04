namespace Ayjet.Evaluation.Center.Application.Features.Candidates.Commands.BulkImport;

// İşlem sonucunu döndürecek DTO
public record BulkImportResultDto(
    int SuccessCount, // Başarıyla eklenen aday sayısı
    int FailedCount,  // Hata nedeniyle eklenemeyen aday sayısı
    List<string> Errors // Hata mesajlarının listesi (örn: "Satır 5: E-posta 'x@y.com' zaten kayıtlı.")
);