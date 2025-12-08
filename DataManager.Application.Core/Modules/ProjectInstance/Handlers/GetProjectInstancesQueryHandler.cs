using DataManager.Application.Contracts.Common;
using DataManager.Application.Contracts.Modules.ProjectInstance;
using DataManager.Application.Core.Common;
using DataManager.Application.Core.Data;
using MediatR;

namespace DataManager.Application.Core.Modules.ProjectInstance.Handlers;

public class GetProjectInstancesQueryHandler : IRequestHandler<GetProjectInstancesQuery, PaginatedList<ProjectInstanceDto>>
{
    private readonly DataManagerDbContext _context;
    private readonly IQueryService<ProjectInstance, Guid> _queryService;

    public GetProjectInstancesQueryHandler(DataManagerDbContext context, IQueryService<ProjectInstance, Guid> queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<PaginatedList<ProjectInstanceDto>> Handle(GetProjectInstancesQuery request, CancellationToken cancellationToken)
    {
        // Prepare query options
        var options = new QueryOptions<ProjectInstance, Guid>
        {
            AsNoTracking = true,
            Filtering = request.Filtering,
            Ordering = request.Ordering
        };

        // No need to pass query - service uses DefaultQuery with DbContext
        var query = await _queryService.PrepareQueryAsync(options: options, cancellationToken: cancellationToken);

        return await _queryService.ExecutePaginatedQueryAsync<ProjectInstanceDto>(
            query,
            request.Pagination,
            (Func<List<ProjectInstance>, List<ProjectInstanceDto>>)(instances => instances.ToDto()),
            cancellationToken);
    }
}
