using DataManager.Application.Contracts.Common;
using DataManager.Application.Contracts.Modules.TranslationsSet;
using DataManager.Application.Core.Common;
using DataManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DataManager.Application.Core.Modules.TranslationsSet.Handlers;

public class GetTranslationsSetsQueryHandler : IRequestHandler<GetTranslationsSetsQuery, PaginatedList<TranslationsSetDto>>
{
    private readonly DataManagerDbContext _context;
    private readonly IQueryService<TranslationsSet, Guid> _queryService;

    public GetTranslationsSetsQueryHandler(DataManagerDbContext context, IQueryService<TranslationsSet, Guid> queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<PaginatedList<TranslationsSetDto>> Handle(GetTranslationsSetsQuery request, CancellationToken cancellationToken)
    {
        // Prepare query options
        var options = new QueryOptions<TranslationsSet, Guid>
        {
            AsNoTracking = true,
            Filtering = request.Filtering,
            Ordering = request.Ordering,
            IncludeFunc = q => q.Include(ds => ds.Includes)
        };

        // No need to pass query - service uses DefaultQuery with DbContext
        var query = await _queryService.PrepareQueryAsync(options: options, cancellationToken: cancellationToken);

        return await _queryService.ExecutePaginatedQueryAsync<TranslationsSetDto>(
            query,
            request.Pagination,
            (Func<List<TranslationsSet>, List<TranslationsSetDto>>)(translationsSets => translationsSets.ToDto()),
            cancellationToken);
    }
}
