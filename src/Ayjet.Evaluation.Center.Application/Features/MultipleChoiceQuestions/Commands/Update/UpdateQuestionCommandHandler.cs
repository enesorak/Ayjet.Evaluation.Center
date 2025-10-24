// UpdateQuestionCommandHandler.cs

using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.MultipleChoiceQuestions.Commands.Update;

public class UpdateQuestionCommandHandler : IRequestHandler<UpdateQuestionCommand>
{
    private readonly IMultipleChoiceQuestionRepository _questionRepo;
    private readonly IRepository<AnswerOption> _optionRepo;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateQuestionCommandHandler(IMultipleChoiceQuestionRepository questionRepo, IRepository<AnswerOption> optionRepo, IUnitOfWork unitOfWork)
    {
        _questionRepo = questionRepo;
        _optionRepo = optionRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateQuestionCommand request, CancellationToken cancellationToken)
    {
        var questionToUpdate = await _questionRepo.GetByIdWithDetailsAsync(request.Id, cancellationToken)
                               ?? throw new NotFoundException(nameof(MultipleChoiceQuestion), request.Id);

        // Ana soru bilgilerini güncelle
        questionToUpdate.QuestionCode = request.QuestionCode;
        questionToUpdate.DisplayOrder = request.DisplayOrder;
        questionToUpdate.DifficultyLevel = request.DifficultyLevel;
        questionToUpdate.Translations.First().Text = request.Text;

        // Mevcut seçenekleri sil (En basit ve güvenilir yöntem)
        foreach (var existingOption in questionToUpdate.Options.ToList())
        {
            _optionRepo.Delete(existingOption);
        }

        // Komuttan gelen yeni seçenekleri ekle
        foreach (var optionDto in request.Options)
        {
            var newOption = new AnswerOption { IsCorrect = optionDto.IsCorrect, CreatedAt = DateTime.UtcNow };
            newOption.Translations.Add(new AnswerOptionTranslation { Text = optionDto.Text, Language = "tr-TR" });
            questionToUpdate.Options.Add(newOption);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}