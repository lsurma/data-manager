using DataManager.Application.Contracts.Common;
using DataManager.Application.Contracts.Modules.DataSets;
using DataManager.Application.Core.Common;
using DataManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DataManager.Application.Core.Modules.DataSets.Handlers;

public class GetDataSetsQueryHandler : IRequestHandler<GetDataSetsQuery, PaginatedList<DataSetDto>>
{
    private readonly DataManagerDbContext _context;
    private readonly IQueryService<DataSet, Guid> _queryService;

    public GetDataSetsQueryHandler(DataManagerDbContext context, IQueryService<DataSet, Guid> queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<PaginatedList<DataSetDto>> Handle(GetDataSetsQuery request, CancellationToken cancellationToken)
    {
        // Prepare query options
        var options = new QueryOptions<DataSet, Guid>
        {
            AsNoTracking = true,
            Filtering = request.Filtering,
            Ordering = request.Ordering,
            IncludeFunc = q => q.Include(ds => ds.Includes)
        };

        // No need to pass query - service uses DefaultQuery with DbContext
        var query = await _queryService.PrepareQueryAsync(options: options, cancellationToken: cancellationToken);

        return await _queryService.ExecutePaginatedQueryAsync<DataSetDto>(
            query,
            request.Pagination,
            (Func<List<DataSet>, List<DataSetDto>>)(dataSets => dataSets.ToDto()),
            cancellationToken);
    }
}
