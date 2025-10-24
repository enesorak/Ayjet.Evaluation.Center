using AutoMapper;
using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.PsychometricQuestions.Queries.GetById;
public class GetPsychometricQuestionDetailQueryHandler : IRequestHandler<GetPsychometricQuestionDetailQuery, PsychometricQuestionDetailDto>
{
    private readonly IPsychometricQuestionRepository _questionRepo;
    private readonly IMapper _mapper;

    public GetPsychometricQuestionDetailQueryHandler(IPsychometricQuestionRepository questionRepo, IMapper mapper)
    {
        _questionRepo = questionRepo;
        _mapper = mapper;
    }

    public async Task<PsychometricQuestionDetailDto> Handle(GetPsychometricQuestionDetailQuery request, CancellationToken cancellationToken)
    {
        var question = await _questionRepo.GetByIdWithDetailsAsync(request.Id, cancellationToken)
                       ?? throw new NotFoundException(nameof(PsychometricQuestion), request.Id);

        return _mapper.Map<PsychometricQuestionDetailDto>(question);
    }
}