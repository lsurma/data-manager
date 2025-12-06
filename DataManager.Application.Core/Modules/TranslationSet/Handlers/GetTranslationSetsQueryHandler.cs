using DataManager.Application.Contracts.Common;
using DataManager.Application.Contracts.Modules.TranslationSet;
using DataManager.Application.Core.Common;
using DataManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DataManager.Application.Core.Modules.TranslationSet.Handlers;

public class GetTranslationSetsQueryHandler : IRequestHandler<GetTranslationSetsQuery, PaginatedList<TranslationSetDto>>
{
    private readonly DataManagerDbContext _context;
    private readonly IQueryService<TranslationSet, Guid> _queryService;

    public GetTranslationSetsQueryHandler(DataManagerDbContext context, IQueryService<TranslationSet, Guid> queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<PaginatedList<TranslationSetDto>> Handle(GetTranslationSetsQuery request, CancellationToken cancellationToken)
    {
        // Prepare query options
        var options = new QueryOptions<TranslationSet, Guid>
        {
            AsNoTracking = true,
            Filtering = request.Filtering,
            Ordering = request.Ordering,
            IncludeFunc = q => q.Include(ds => ds.Includes)
        };

        // No need to pass query - service uses DefaultQuery with DbContext
        var query = await _queryService.PrepareQueryAsync(options: options, cancellationToken: cancellationToken);

        return await _queryService.ExecutePaginatedQueryAsync<TranslationSetDto>(
            query,
            request.Pagination,
            (Func<List<TranslationSet>, List<TranslationSetDto>>)(translationSets => translationSets.ToDto()),
            cancellationToken);
    }
}
