using DataManager.Application.Contracts.Modules.DataSet;
using DataManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DataManager.Application.Core.Modules.DataSet.Handlers;

public class ResolveDataSetQueryHandler : IRequestHandler<ResolveDataSetQuery, DataSet?>
{
    private readonly DataManagerDbContext _context;

    public ResolveDataSetQueryHandler(DataManagerDbContext context)
    {
        _context = context;
    }

    public async Task<DataSet?> Handle(ResolveDataSetQuery request, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(request.NameOrId, out var dataSetId))
        {
            return await _context.DataSets
                .AsNoTracking()
                .FirstOrDefaultAsync(ds => ds.Id == dataSetId, cancellationToken);
        }

        return await _context.DataSets
            .AsNoTracking()
            .FirstOrDefaultAsync(ds => ds.Name == request.NameOrId, cancellationToken);
    }
}
