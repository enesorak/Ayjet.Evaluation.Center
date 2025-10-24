using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Application.Common.Models;
using Ayjet.Evaluation.Center.Application.Extensions;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Enums;
using MediatR;

// OrderBy için gerekli

namespace Ayjet.Evaluation.Center.Application.Features.TestDefinitions.Queries.GetQuestionsByTestDefinition;

public class GetQuestionsByTestDefinitionQueryHandler : IRequestHandler<GetQuestionsByTestDefinitionQuery, PagedList<QuestionDto>>
{
    private readonly ITestDefinitionRepository _testDefinitionRepo;
    private readonly IPsychometricQuestionRepository _psychometricQuestionRepo;
    private readonly IMultipleChoiceQuestionRepository _mcqRepo;

    public GetQuestionsByTestDefinitionQueryHandler(
        ITestDefinitionRepository testDefinitionRepo,
        IPsychometricQuestionRepository psychometricQuestionRepo,
        IMultipleChoiceQuestionRepository mcqRepo)
    {
        _testDefinitionRepo = testDefinitionRepo;
        _psychometricQuestionRepo = psychometricQuestionRepo;
        _mcqRepo = mcqRepo;
    }

    public async Task<PagedList<QuestionDto>> Handle(GetQuestionsByTestDefinitionQuery request, CancellationToken cancellationToken)
    {
        var testDefinition = await _testDefinitionRepo.GetByIdAsync(request.TestDefinitionId, cancellationToken)
            ?? throw new NotFoundException(nameof(TestDefinition), request.TestDefinitionId);

        var isPsychometric = testDefinition.Type == TestType.Psychometric;

        // Sayfalama için toplam öğe sayısını ve mevcut sayfanın öğelerini almamız gerekiyor.
        // Repository'den IQueryable alıp burada işlemek en verimli yoldur.

        if (isPsychometric)
        {
            var query = _psychometricQuestionRepo.GetListByTestDefinitionId(request.TestDefinitionId, request.SearchTerm);
            var pagedResult = await query.OrderBy(q => q.DisplayOrder).ToPagedListAsync(request.PageParams.PageNumber, request.PageParams.PageSize, cancellationToken);
            
            var questionDtos = pagedResult.Items.Select(q =>

            {
                var translation = q.Translations.FirstOrDefault(t => t.Language == "en-US") 
                                  ?? q.Translations.FirstOrDefault();

                return new QuestionDto
                { 

                    Id = q.Id,
                    Text = translation?.Text ?? "N/A",
                    Language = translation?.Language ?? "N/A",
                    DisplayOrder = q.DisplayOrder,
                    IsActive = q.IsActive,
                    QuestionCode = null, // Psychometric'te bu alan yok
                    TestType = testDefinition.Type.ToString(),
                    Options = null // Psikometrikte seçenekleri ayrıca oluşturuyoruz
                };
            })
                
              .ToList();

            return new PagedList<QuestionDto>(
                questionDtos,
                pagedResult.TotalCount,
                pagedResult.PageNumber,
                pagedResult.PageSize);
        }
        else // MultipleChoice
        {
            var query = _mcqRepo.GetListByTestDefinitionId(request.TestDefinitionId, request.SearchTerm);
            var pagedResult = await query.OrderBy(q => q.DisplayOrder).ToPagedListAsync(request.PageParams.PageNumber, request.PageParams.PageSize, cancellationToken);
            
            var questionDtos = pagedResult.Items.Select(q => new QuestionDto
            {
                Id = q.Id,
                Text = q.Translations.FirstOrDefault()?.Text ?? "N/A",
                Language = q.Translations.FirstOrDefault()?.Language ?? "N/A",
                DisplayOrder = q.DisplayOrder,
                IsActive = q.IsActive,
                QuestionCode = q.QuestionCode,
                TestType = testDefinition.Type.ToString(),
                Options = q.Options.Select(o => new OptionDto
                {
                    Id = o.Id,
                    Text = o.Translations.FirstOrDefault()?.Text ?? "N/A",
                    IsCorrect = o.IsCorrect
                }).ToList()
            }).ToList();
            
            return new PagedList<QuestionDto>(questionDtos, pagedResult.TotalCount, pagedResult.PageNumber, pagedResult.PageSize);;
        }
    }
}