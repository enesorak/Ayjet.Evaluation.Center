namespace Ayjet.Evaluation.Center.Application.Features.Candidates.Commands.BulkImportPhotos;

public record BulkImportPhotosResultDto(
    int TotalFilesFound,    // ZIP içinde toplam kaç fotoğraf bulundu
    int SuccessfullyMatched, // Başarıyla eşleşen ve güncellenen aday sayısı
    List<string> MatchFailures  // Eşleşemeyen dosya adlarının listesi
);