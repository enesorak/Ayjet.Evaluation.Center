using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Application.Common.Models;
using Ayjet.Evaluation.Center.Application.Extensions;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.Candidates.Queries.GetList;

public class GetCandidateListQueryHandler : IRequestHandler<GetCandidateListQuery, PagedList<CandidateDto>>
{
    private readonly ICandidateRepository _candidateRepo;
    private readonly IMapper _mapper;
    public GetCandidateListQueryHandler(ICandidateRepository repo, IMapper mapper) 
        => (_candidateRepo, _mapper) = (repo, mapper);

    public async Task<PagedList<CandidateDto>> Handle(GetCandidateListQuery request, CancellationToken ct)
    {
        var candidatesQuery = _candidateRepo.GetAsQueryable();

        // Arşiv durumuna göre filtrele (varsayılan olarak sadece aktifleri göster)
        candidatesQuery = candidatesQuery.Where(c => c.IsArchived == (request.IsArchived ?? false));

        // Departmana göre filtrele
        if (request.Department.HasValue)
        {
            candidatesQuery = candidatesQuery.Where(c => c.Department == request.Department.Value);
        }
        
        
        // Atama durumuna göre filtrele
        if (request.AssignmentStatus.HasValue)
        {
            // Test ataması olmayanları veya olanları filtrelemek için daha karmaşık bir mantık gerekebilir.
            // Şimdilik, en az bir tane o durumda ataması olanları getirelim.
            candidatesQuery = candidatesQuery.Where(c => 
                c.TestAssignments.Any(a => a.Status == request.AssignmentStatus.Value));
        }
        
        

        // Arama terimine göre filtrele (Ad, Soyad, E-posta, Kodlar)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            candidatesQuery = candidatesQuery.Where(c =>
                c.FirstName.ToLower().Contains(term) ||
                c.LastName.ToLower().Contains(term) ||
                c.Email.ToLower().Contains(term) ||
                (c.InitialCode != null && c.InitialCode.ToLower().Contains(term)) ||
                (c.FleetCode != null && c.FleetCode.ToLower().Contains(term))
            );
        }
        
        var orderedQuery = candidatesQuery.OrderBy(c => c.LastName).ThenBy(c => c.FirstName);
        var dtoQuery = orderedQuery.ProjectTo<CandidateDto>(_mapper.ConfigurationProvider);


 

        return await dtoQuery.ToPagedListAsync(request.PageParams.PageNumber, request.PageParams.PageSize, ct);
    }
}