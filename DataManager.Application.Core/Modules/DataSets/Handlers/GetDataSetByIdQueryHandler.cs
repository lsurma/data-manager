using DataManager.Application.Contracts.Modules.DataSets;
using DataManager.Application.Core.Common;
using DataManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DataManager.Application.Core.Modules.DataSets.Handlers;

public class GetDataSetByIdQueryHandler : IRequestHandler<GetDataSetByIdQuery, DataSetDto?>
{
    private readonly DataManagerDbContext _context;
    private readonly DataSetsQueryService _queryService;

    public GetDataSetByIdQueryHandler(DataManagerDbContext context, DataSetsQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<DataSetDto?> Handle(GetDataSetByIdQuery request, CancellationToken cancellationToken)
    {
        var options = new QueryOptions<DataSet, Guid>
        {
            AsNoTracking = true,
            IncludeFunc = q => q.Include(ds => ds.Includes)
        };

        var dataSet = await _queryService.GetByIdAsync(
            request.Id,
            options: options,
            cancellationToken: cancellationToken
        );

        return dataSet?.ToDto();
    }
}
