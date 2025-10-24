using AutoMapper;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.Candidates.Queries.GetCounts;

public class GetCandidateCountsQueryHandler : IRequestHandler<GetCandidateCountsQuery, CandidateCountsDto>
{
    private readonly ICandidateRepository _candidateRepository;
    private readonly IMapper _mapper;

    public GetCandidateCountsQueryHandler(ICandidateRepository candidateRepository, IMapper mapper)
    {
        _candidateRepository = candidateRepository;
        _mapper = mapper;
    }
    public async Task<CandidateCountsDto> Handle(GetCandidateCountsQuery request, CancellationToken cancellationToken)
    {
        var counts = await _candidateRepository.GetCandidateCountsAsync(cancellationToken);
        return _mapper.Map<CandidateCountsDto>(counts);
    }
}