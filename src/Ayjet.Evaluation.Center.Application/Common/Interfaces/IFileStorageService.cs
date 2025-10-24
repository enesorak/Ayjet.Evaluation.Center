namespace Ayjet.Evaluation.Center.Application.Common.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Dosyayı kaydeder ve erişilebilecek public URL'ini döndürür.
    /// </summary>
    Task<string> SaveFileAsync(Stream fileStream, string fileName);
}
