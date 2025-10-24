using AutoMapper;
using AutoMapper.QueryableExtensions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Application.Common.Models;
using Ayjet.Evaluation.Center.Application.Extensions;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestDefinitions.Queries.GetList;
public class GetTestDefinitionListQueryHandler : IRequestHandler<GetTestDefinitionListQuery, PagedList<TestDefinitionDto>>
{
    private readonly ITestDefinitionRepository _repository;
    private readonly IMapper _mapper;

    public GetTestDefinitionListQueryHandler(ITestDefinitionRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedList<TestDefinitionDto>> Handle(GetTestDefinitionListQuery request, CancellationToken cancellationToken)
    {
        // 1. Filtrelenebilir sorguyu al
        var query = _repository.GetAllWithDetails();
        
        // 2. Veritabanından sadece DTO için gereken kolonları çekecek şekilde map'le
        var dtoQuery = query.ProjectTo<TestDefinitionDto>(_mapper.ConfigurationProvider);

        // 3. Sayfalama helper metodumuzu kullanarak sonucu oluştur
        return await dtoQuery.ToPagedListAsync(
            request.PageParams.PageNumber, 
            request.PageParams.PageSize, 
            cancellationToken
        );
    }
}