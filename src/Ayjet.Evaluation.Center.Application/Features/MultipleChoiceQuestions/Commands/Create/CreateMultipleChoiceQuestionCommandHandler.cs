using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.MultipleChoiceQuestions.Commands.Create;

public class CreateMultipleChoiceQuestionCommandHandler : IRequestHandler<CreateMultipleChoiceQuestionCommand, int>
{
    private readonly IMultipleChoiceQuestionRepository _questionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateMultipleChoiceQuestionCommandHandler(IMultipleChoiceQuestionRepository questionRepository, IUnitOfWork unitOfWork)
    {
        _questionRepository = questionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(CreateMultipleChoiceQuestionCommand request, CancellationToken cancellationToken)
    {
        var displayOrder = request.DisplayOrder ?? (await _questionRepository.GetMaxDisplayOrderAsync(request.TestDefinitionId!) + 1);

        var questionCode = request.QuestionCode;
        // Eğer kullanıcı bir kod girmediyse, otomatik olarak oluştur.
        if (string.IsNullOrWhiteSpace(questionCode))
        {
            var questionCount = await _questionRepository.CountAsync(cancellationToken);
            questionCode = $"AYJ-MC-{(questionCount + 1).ToString("D4")}"; // Örn: AYJ-MC-0001
        }
 
        
        var question = new MultipleChoiceQuestion
        {
            TestDefinitionId = request.TestDefinitionId!,
            QuestionCode = questionCode,
            DisplayOrder = displayOrder,
            DifficultyLevel = request.DifficultyLevel,
            CreatedAt = DateTime.UtcNow
        };

        question.Translations.Add(new MultipleChoiceQuestionTranslation 
        {
            Text = request.Text,
            Language = request.Language
        });
        foreach (var optionDto in request.Options)
        {
            var option = new AnswerOption { IsCorrect = optionDto.IsCorrect, CreatedAt = DateTime.UtcNow };
            option.Translations.Add(new AnswerOptionTranslation { Text = optionDto.Text, Language = request.Language });
            question.Options.Add(option);
        }

        await _questionRepository.AddAsync(question, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return question.Id;
    }
}