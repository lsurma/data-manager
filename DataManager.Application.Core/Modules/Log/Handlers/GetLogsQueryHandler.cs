using DataManager.Application.Contracts.Common;
using DataManager.Application.Contracts.Modules.Log;
using DataManager.Application.Core.Common;
using DataManager.Application.Core.Data;
using MediatR;

namespace DataManager.Application.Core.Modules.Log.Handlers;

public class GetLogsQueryHandler : IRequestHandler<GetLogsQuery, PaginatedList<LogDto>>
{
    private readonly DataManagerDbContext _context;
    private readonly LogsQueryService _queryService;

    public GetLogsQueryHandler(DataManagerDbContext context, LogsQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<PaginatedList<LogDto>> Handle(GetLogsQuery request, CancellationToken cancellationToken)
    {
        // Prepare query options
        var options = new QueryOptions<Log, Guid>
        {
            AsNoTracking = true,
            Filtering = request.Filtering,
            Ordering = request.Ordering
        };

        // No need to pass query - service uses DefaultQuery with DbContext
        var query = await _queryService.PrepareQueryAsync(options: options, cancellationToken: cancellationToken);

        return await _queryService.ExecutePaginatedQueryAsync<LogDto>(
            query,
            request.Pagination,
            (Func<List<Log>, List<LogDto>>)(logs => logs.ToDto()),
            cancellationToken);
    }
}
