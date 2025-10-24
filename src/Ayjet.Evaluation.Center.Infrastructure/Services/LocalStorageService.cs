using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace Ayjet.Evaluation.Center.Infrastructure.Services;

public class LocalStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;
    public LocalStorageService(IWebHostEnvironment env) { _env = env; }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName)
    {
        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads/profiles");
        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        await using (var newFileStream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(newFileStream);
        }

        // Dönen URL, public olarak erişilebilir olmalı.
        return $"/uploads/profiles/{uniqueFileName}";
    }
}