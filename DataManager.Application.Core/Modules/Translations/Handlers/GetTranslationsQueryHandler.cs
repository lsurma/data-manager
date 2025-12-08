using DataManager.Application.Contracts.Common;
using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Application.Core.Common;
using DataManager.Application.Core.Data;
using MediatR;

namespace DataManager.Application.Core.Modules.Translations.Handlers;

public class GetTranslationsQueryHandler : IRequestHandler<GetTranslationsQuery, PaginatedList<TranslationDto>>
{
    private readonly DataManagerDbContext _context;
    private readonly TranslationsQueryService _queryService;

    public GetTranslationsQueryHandler(DataManagerDbContext context, TranslationsQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<PaginatedList<TranslationDto>> Handle(GetTranslationsQuery request, CancellationToken cancellationToken)
    {
        // Create complete query specification with all configuration in one place
        var options = new QueryOptions<Translation, Guid, TranslationDto>
        {
            AsNoTracking = true,
            Filtering = request.Filtering,
            Ordering = request.Ordering,
            Selector = t => new TranslationDto
            {
                Id = t.Id,
                InternalGroupName1 = t.InternalGroupName1,
                InternalGroupName2 = t.InternalGroupName2,
                ResourceName = t.ResourceName,
                TranslationName = t.TranslationName,
                CultureName = t.CultureName,
                Content = t.Content,
                ContentTemplate = t.ContentTemplate,
                DataSetId = t.DataSetId,
                LayoutId = t.LayoutId,
                IsCurrentVersion = t.IsCurrentVersion,
                IsDraftVersion = t.IsDraftVersion,
                IsOldVersion = t.IsOldVersion,
                OriginalTranslationId = t.OriginalTranslationId,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                CreatedBy = t.CreatedBy
            }
        };

        // Apply query preparation (authorization, filters, ordering)
        // No need to pass query - service uses DefaultQuery
        var query = await _queryService.PrepareQueryAsync(options: options, cancellationToken: cancellationToken);

        // Execute paginated query with projection
        return await _queryService.ExecutePaginatedQueryAsync(
            query,
            request.Pagination,
            options,
            cancellationToken);
    }
}
