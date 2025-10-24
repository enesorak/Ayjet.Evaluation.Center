using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using ClosedXML.Excel;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.MultipleChoiceQuestions.Commands.BulkImport;

public class BulkImportQuestionsCommandHandler : IRequestHandler<BulkImportQuestionsCommand, ImportResultDto>
{
    private readonly IMultipleChoiceQuestionRepository _questionRepo;
    private readonly IUnitOfWork _unitOfWork;

    public BulkImportQuestionsCommandHandler(IMultipleChoiceQuestionRepository questionRepo, IUnitOfWork unitOfWork)
    {
        _questionRepo = questionRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<ImportResultDto> Handle(BulkImportQuestionsCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var successCount = 0;
        var questionsToAdd = new List<MultipleChoiceQuestion>();

        if (request.ExcelFile == null || request.ExcelFile.Length == 0)
        {
            return new ImportResultDto(0, 1, new List<string> { "Lütfen bir dosya yükleyin." });
        }

        using (var stream = request.ExcelFile.OpenReadStream())
        using (var workbook = new XLWorkbook(stream)) // EPPlus -> ClosedXML
        {
            var worksheet = workbook.Worksheet(1); // İlk çalışma sayfasını al
            if (worksheet == null)
                return new ImportResultDto(0, 1, new List<string> { "Excel dosyası boş veya okunamıyor." });

            // Başlık satırını atlayarak tüm dolu satırları al
            var rows = worksheet.RowsUsed().Skip(1);

            var maxOrder = await _questionRepo.GetMaxDisplayOrderAsync(request.TestDefinitionId);
            var totalQuestions = await _questionRepo.CountAsync(cancellationToken);

            foreach (var row in rows)
            {
                var rowNumber = row.RowNumber();
                try
                {
                    var text = row.Cell(1).GetValue<string>()?.Trim();
                    var language = row.Cell(2).GetValue<string>()?.Trim() ?? "tr-TR";
                    var questionCode = row.Cell(3).GetValue<string>()?.Trim();
                    var displayOrder = row.Cell(4).GetValue<int?>();
                    var difficulty = row.Cell(5).GetValue<int?>() ?? 3;
                    var correctOptionNumber = row.Cell(6).GetValue<int?>();

                    var optionsText = Enumerable.Range(7, 5)
                        .Select(col => row.Cell(col).GetValue<string>()?.Trim())
                        .Where(o => !string.IsNullOrWhiteSpace(o))
                        .ToList();

                    if (string.IsNullOrWhiteSpace(text) || optionsText.Count < 2 || !correctOptionNumber.HasValue ||
                        correctOptionNumber < 1 || correctOptionNumber > optionsText.Count)
                    {
                        errors.Add(
                            $"{rowNumber}. satır: 'SoruMetni', en az 2 'Seçenek' ve geçerli bir 'DogruCevapNo' alanları zorunludur.");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(questionCode))
                    {
                        questionCode = $"AYJ-MC-{(totalQuestions + successCount + 1).ToString("D4")}";
                    }

                    if (!displayOrder.HasValue)
                    {
                        displayOrder = maxOrder + successCount + 1;
                    }

                    var question = new MultipleChoiceQuestion
                    {
                        TestDefinitionId = request.TestDefinitionId,
                        DisplayOrder = displayOrder.Value,
                        QuestionCode = questionCode,
                        DifficultyLevel = difficulty,
                    };
                    question.Translations.Add(
                        new MultipleChoiceQuestionTranslation { Text = text, Language = language });

                    for (int i = 0; i < optionsText.Count; i++)
                    {
                        var newOption = new AnswerOption { IsCorrect = (i + 1) == correctOptionNumber.Value };
                        
                        newOption.Translations.Add(new AnswerOptionTranslation { Text = optionsText[i]!, Language = language });
 
                        question.Options.Add(newOption);
                    }

                    questionsToAdd.Add(question);
                    successCount++;
                }
                catch (Exception ex)
                {
                    errors.Add($"{rowNumber}. satır işlenirken bir hata oluştu: {ex.Message}");
                }
            }
        }

        if (questionsToAdd.Any())
        {
            await _questionRepo.AddRangeAsync(questionsToAdd, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new ImportResultDto(successCount, errors.Count, errors);
    }
}