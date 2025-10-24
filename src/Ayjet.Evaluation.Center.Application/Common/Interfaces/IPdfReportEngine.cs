using Ayjet.Evaluation.Center.Application.Features.TestResults.Models; // BaseReportModel için

namespace Ayjet.Evaluation.Center.Application.Common.Interfaces;

// PDF raporu oluşturma işlemini soyutlayan arayüz
public interface IPdfReportEngine
{
    // Verilen rapor modeline göre PDF dosyasını byte dizisi olarak üretir.
    byte[] GenerateReportPdf(BaseReportModel reportModel);

    // Gelecekte farklı formatlar için metotlar eklenebilir:
    // byte[] GenerateReportExcel(BaseReportModel reportModel);
}