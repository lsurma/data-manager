using DataManager.Application.Contracts.Modules.Log;
using DataManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DataManager.Application.Core.Modules.Log.Handlers;

public class GetLogByIdQueryHandler : IRequestHandler<GetLogByIdQuery, LogDto?>
{
    private readonly DataManagerDbContext _context;
    private readonly LogsQueryService _queryService;

    public GetLogByIdQueryHandler(DataManagerDbContext context, LogsQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<LogDto?> Handle(GetLogByIdQuery request, CancellationToken cancellationToken)
    {
        // Apply authorization filtering
        var query = await _queryService.ApplyAuthorizationAsync(_context.Logs, cancellationToken);
        
        var log = await query
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        return log?.ToDto();
    }
}
