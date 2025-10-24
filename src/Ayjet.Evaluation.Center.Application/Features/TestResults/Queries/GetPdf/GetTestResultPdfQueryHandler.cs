using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces; // IPdfReportEngine için
using Ayjet.Evaluation.Center.Application.Features.Candidates.Queries.GetList; // CandidateDto için
using Ayjet.Evaluation.Center.Application.Features.TestResults.Models; // MmpiReportModel için
using Ayjet.Evaluation.Center.Application.Services.Scoring; // MMPIScoringResult için
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Enums;
using AutoMapper;
using MediatR;
using System.Text.Json;

namespace Ayjet.Evaluation.Center.Application.Features.TestResults.Queries.GetPdf;

public class GetTestResultPdfQueryHandler : IRequestHandler<GetTestResultPdfQuery, byte[]>
{
    private readonly ITestResultRepository _resultRepository;
    private readonly IMapper _mapper;
    private readonly IPdfReportEngine _pdfEngine; // <-- YENİ BAĞIMLILIK

    public GetTestResultPdfQueryHandler(
        ITestResultRepository resultRepository,
        IMapper mapper,
        IPdfReportEngine pdfEngine) // <-- CONSTRUCTOR'A EKLENDİ
    {
        _resultRepository = resultRepository;
        _mapper = mapper;
        _pdfEngine = pdfEngine; // <-- ATAMA YAPILDI
    }

    public async Task<byte[]> Handle(GetTestResultPdfQuery request, CancellationToken cancellationToken)
    {
        var testResult = await _resultRepository.GetByAssignmentIdAsync(request.AssignmentId, cancellationToken)
            ?? throw new NotFoundException($"Test result not found for assignment ID: {request.AssignmentId}");

        BaseReportModel reportModel; // Soyut model kullanıyoruz

        // Test tipine göre doğru rapor modelini oluştur
        if (testResult.TestAssignment.TestDefinition.Type == TestType.Psychometric)
        {
            MMPIScoringResult? scores;
            try { scores = JsonSerializer.Deserialize<MMPIScoringResult>(testResult.ResultsPayloadJson ?? "{}"); }
            catch (JsonException ex) { throw new Exception($"Failed to parse MMPI scores: {ex.Message}"); }
            if (scores == null) { throw new Exception("MMPI scores are missing or invalid."); }

            reportModel = new MmpiReportModel
            {
                Candidate = _mapper.Map<CandidateDto>(testResult.TestAssignment.Candidate),
                TestTitle = testResult.TestAssignment.TestDefinition.Title,
                CompletedAt = testResult.TestAssignment.CompletedAt,
                Scores = scores
            };
        }
        else if (testResult.TestAssignment.TestDefinition.Type == TestType.MultipleChoice)
        {
             // TODO: Çoktan seçmeli için ExamReportModel ve QuestPDF dokümanı oluşturulunca burası doldurulacak.
             // Şimdilik hata fırlatalım.
             throw new NotSupportedException("PDF report generation for Multiple Choice tests is not yet implemented.");

             // Örnek:
             // var examScores = JsonSerializer.Deserialize<ExamScoringResult>(testResult.ResultsPayloadJson ?? "{}");
             // reportModel = new ExamReportModel { /* ... verileri doldur ... */ };
        }
        else
        {
            throw new NotSupportedException($"Unsupported test type for PDF report: {testResult.TestAssignment.TestDefinition.Type}");
        }

        // PDF Motorunu çağır ve sonucu döndür
        byte[] pdfBytes = _pdfEngine.GenerateReportPdf(reportModel);

        return pdfBytes;
    }
}