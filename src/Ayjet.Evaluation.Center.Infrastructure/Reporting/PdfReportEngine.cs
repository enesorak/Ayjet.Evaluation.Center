 
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Application.Features.TestResults.Models;
using Ayjet.Evaluation.Center.Infrastructure.Reporting.Documents;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure; // GeneratePdf için

namespace Ayjet.Evaluation.Center.Infrastructure.Reporting;


public class PdfReportEngine : IPdfReportEngine
{
    public byte[] GenerateReportPdf(BaseReportModel reportModel)
    {
        // Gelen modelin tipine göre doğru QuestPDF dokümanını seç ve oluştur.
        
        IDocument document = reportModel switch
        {
            
            
            
            MmpiReportModel mmpiModel => new MmpiReportDocument(mmpiModel),
            // Gelecekte eklenecek diğer rapor tipleri için case'ler:
            // ExamReportModel examModel => new ExamReportDocument(examModel),
            _ => throw new NotSupportedException($"Report generation not supported for model type: {reportModel.GetType().Name}")
        };

        // QuestPDF dokümanını byte dizisine çevir ve döndür.
        return document.GeneratePdf();
    }
}