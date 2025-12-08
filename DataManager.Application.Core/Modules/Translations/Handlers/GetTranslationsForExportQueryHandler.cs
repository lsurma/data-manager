using DataManager.Application.Contracts.Common;
using DataManager.Application.Contracts.Modules.Translations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DataManager.Application.Core.Modules.Translations.Handlers;

/// <summary>
/// Handler for simplified translation export query
/// Uses TranslationsQueryService internal methods for efficient querying
/// </summary>
public class GetTranslationsForExportQueryHandler : IRequestHandler<GetTranslationsForExportQuery, PaginatedList<SimpleTranslationDto>>
{
    private readonly TranslationsQueryService _queryService;

    public GetTranslationsForExportQueryHandler(TranslationsQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<PaginatedList<SimpleTranslationDto>> Handle(GetTranslationsForExportQuery request, CancellationToken cancellationToken)
    {
        // Prepare query options with translation set filter
        var pagination = new PaginationParameters(request.Offset, request.Limit);
        var queryOptions = new TranslationsQueryService.Options
        {
            Filtering = new FilteringParameters
            {
                QueryFilters = new List<IQueryFilter>
                {
                    new DataSetIdFilter(request.DataSetId)
                }
            }
        };

        // Prepare the query using the service's internal methods
        // This automatically applies authorization and default filters
        var query = await _queryService.PrepareQueryAsync(options: queryOptions, cancellationToken: cancellationToken);

        // Apply culture filters if provided
        if (request.Cultures.Any())
        {
            var cultureList = request.Cultures.ToList();
            query = query.Where(t => t.CultureName != null && cultureList.Contains(t.CultureName));
        }
        
        if(request.ContentUpdatedAtAfter != null)
        {
            query = query.Where(t => t.ContentUpdatedAt >= request.ContentUpdatedAtAfter);
        }

        // Pobieramy zestaw danych z innego zestawu niż "SpecificDataSetId", wiec musimy wyłączyć translacje które są wlaśnie w tym "SpecificDataSetId"
        // Bo tutaj pobieramy bazowe translacje, bazowy zestaw
        if (request.SpecificDataSetId != null && request.SpecificDataSetId != request.DataSetId)
        {
            var translationKeysInSpecificSetQuery = await _queryService.PrepareQueryAsync(
                options: new TranslationsQueryService.Options(),
                cancellationToken: cancellationToken
            );
            translationKeysInSpecificSetQuery = translationKeysInSpecificSetQuery.Where(q => q.DataSetId == request.SpecificDataSetId);
            var translationKeysInSpecificSet = await translationKeysInSpecificSetQuery
                .Select(t => t.TranslationKey)
                .ToListAsync(cancellationToken);

            // Wyłączamy translacje które są w "SpecificDataSetId" - czyli te bardziej specyficzne, ich nie chcemy pobierać
            query = query.Where(t => !translationKeysInSpecificSet.Contains(t.TranslationKey));
        }

        // Apply ordering by ContentUpdatedAt descending (standard for exports)
        query = query.OrderByDescending(t => t.ContentUpdatedAt);

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var translations = await query
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .ToListAsync(cancellationToken);

        // Map to SimpleTranslationDto
        var simpleDtos = translations.ToSimpleDto();

        // Return paginated result
        return new PaginatedList<SimpleTranslationDto>(
            simpleDtos,
            totalCount,
            pagination.PageNumber,
            pagination.PageSize
        );
    }
}
