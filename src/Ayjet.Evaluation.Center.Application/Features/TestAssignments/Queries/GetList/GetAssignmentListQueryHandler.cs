using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Application.Common.Models;
using Ayjet.Evaluation.Center.Application.Extensions;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Queries.GetList;

public class GetAssignmentListQueryHandler : IRequestHandler<GetAssignmentListQuery, PagedList<TestAssignmentDto>>
{
    private readonly ITestAssignmentRepository _assignmentRepository;
    private readonly IMapper _mapper;
    public GetAssignmentListQueryHandler(ITestAssignmentRepository repo, IMapper mapper)
        => (_assignmentRepository, _mapper) = (repo, mapper);


    public async Task<PagedList<TestAssignmentDto>> Handle(GetAssignmentListQuery request, CancellationToken ct)
    {
        var query = _assignmentRepository.GetAllWithDetails();

        if (request.Status.HasValue)
        {
            query = query.Where(a => a.Status == request.Status.Value);
        }

        // ProjectTo, veritabanından sadece DTO için gereken kolonları çeker (performans artışı)
        var dtoQuery = query.ProjectTo<TestAssignmentDto>(_mapper.ConfigurationProvider);

        // Extension metodumuzla sayfalanmış listeyi oluştur
        return await dtoQuery.ToPagedListAsync(request.PageParams.PageNumber, request.PageParams.PageSize, ct);

    }
}