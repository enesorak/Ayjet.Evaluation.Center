using AutoMapper;
using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Application.Features.Candidates.Queries.GetList;
using Ayjet.Evaluation.Center.Domain.Entities;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.Candidates.Queries.GetById;

public class GetCandidateByIdQueryHandler : IRequestHandler<GetCandidateByIdQuery, CandidateDto>
{
    private readonly ICandidateRepository _candidateRepo;
    private readonly IMapper _mapper;
    public GetCandidateByIdQueryHandler(ICandidateRepository repo, IMapper mapper) 
        => (_candidateRepo, _mapper) = (repo, mapper);

    public async Task<CandidateDto> Handle(GetCandidateByIdQuery request, CancellationToken ct)
    {
        var candidate = await _candidateRepo.GetByIdAsync(request.Id, ct) 
                        ?? throw new NotFoundException(nameof(Candidate), request.Id);
        return _mapper.Map<CandidateDto>(candidate);
    }
}