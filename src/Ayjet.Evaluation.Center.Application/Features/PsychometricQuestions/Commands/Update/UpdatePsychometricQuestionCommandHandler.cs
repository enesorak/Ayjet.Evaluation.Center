using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.PsychometricQuestions.Commands.Update;

 public class UpdatePsychometricQuestionCommandHandler : IRequestHandler<UpdatePsychometricQuestionCommand>
{
    private readonly IPsychometricQuestionRepository _questionRepo;
    // QuestionScaleMapping işlemleri için genel repository'yi kullanabiliriz.
    private readonly IRepository<QuestionScaleMapping> _mappingRepo;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePsychometricQuestionCommandHandler(
        IPsychometricQuestionRepository questionRepo, 
        IRepository<QuestionScaleMapping> mappingRepo, 
        IUnitOfWork unitOfWork)
    {
        _questionRepo = questionRepo;
        _mappingRepo = mappingRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdatePsychometricQuestionCommand request, CancellationToken cancellationToken)
    {
        var questionToUpdate = await _questionRepo.GetByIdWithDetailsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(PsychometricQuestion), request.Id);

        // 1. Ana soru bilgilerini güncelle
        questionToUpdate.DisplayOrder = request.DisplayOrder;
        //questionToUpdate.QuestionCode = request.QuestionCode;
        questionToUpdate.IsActive = request.IsActive;
        questionToUpdate.UpdatedAt = DateTime.UtcNow;

        // 2. Çevirileri güncelle veya ekle
        foreach (var transDto in request.Translations)
        {
            var translation = questionToUpdate.Translations.FirstOrDefault(t => t.Language == transDto.Language);
            if (translation != null)
            {
                translation.Text = transDto.Text;
            }
            else
            {
                questionToUpdate.Translations.Add(new PsychometricQuestionTranslation { Language = transDto.Language, Text = transDto.Text });
            }
        }

        // 3. Puanlama kurallarını güncelle (Eskileri sil, yenileri ekle)
        _mappingRepo.DeleteRange(questionToUpdate.ScaleMappings);
        
        var newMappings = request.ScaleMappings.Select(dto => new QuestionScaleMapping
        {
            PsychometricQuestionId = questionToUpdate.Id,
            PsychometricScaleId = dto.ScaleId,
            ScoringDirection = dto.ScoringDirection,
            RequiredGender = dto.RequiredGender
        }).ToList();

        if (newMappings.Any())
        {
            await _mappingRepo.AddRangeAsync(newMappings, cancellationToken);
        }
        
        // 4. Tüm değişiklikleri tek bir işlemde kaydet
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}