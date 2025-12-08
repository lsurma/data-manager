using DataManager.Application.Contracts.Common;
using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Application.Core.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DataManager.Application.Core.Modules.Translations.Handlers;

public class ExportTranslationsQueryHandler : IRequestHandler<ExportTranslationsQuery, Stream>
{
    private readonly TranslationsQueryService _queryService;
    private readonly TranslationExporterFactory _exporterFactory;

    public ExportTranslationsQueryHandler(TranslationsQueryService queryService, TranslationExporterFactory exporterFactory)
    {
        _queryService = queryService;
        _exporterFactory = exporterFactory;
    }

    public async Task<Stream> Handle(ExportTranslationsQuery request, CancellationToken cancellationToken)
    {
        var queryOptions = new TranslationsQueryService.Options
        {
            Ordering = new OrderingParameters
            {
                OrderBy = request.OrderBy,
                OrderDirection = request.OrderDirection
            },
            Filtering = new FilteringParameters
            {
                QueryFilters = new List<IQueryFilter>()
                {
                    new DataSetIdFilter(request.DataSetId),
                    new CultureNameFilter(request.TargetCulture)
                }
            }
        };

        // Fetch the main translations to export
        var query = await _queryService.PrepareQueryAsync(options: queryOptions, cancellationToken: cancellationToken);
        var translations = await query.ToListAsync(cancellationToken);
        var translationExportDtos = translations.ToExportDto();

        // Fetch base language translations for the same keys
        // Extract unique keys (ResourceName, TranslationName) from the main translations
        var translationKeys = translations
            .Select(t => new TranslationKey(t.ResourceName, t.TranslationName))
            .ToHashSet();

        // Build a query for base language translations
        List<Translation> baseCultureTranslations = new List<Translation>();
        if (translationKeys.Any())
        {
            // Create a filter options for base language
            var baseCultureQueryOptions = new TranslationsQueryService.Options
            {
                Filtering = new FilteringParameters
                {
                    QueryFilters = new List<IQueryFilter>
                    {
                        new CultureNameFilter(request.BaseCulture)
                    }
                }
            };

            var baseQuery = await _queryService.PrepareQueryAsync(options: baseCultureQueryOptions, cancellationToken: cancellationToken);
            
            // Filter to only include translations with matching keys using Contains for better SQL performance
            // We use separate Contains operations for ResourceName and TranslationName to leverage SQL indexes
            // This may match more records than needed (e.g., ResourceA+TranslationB when we only want ResourceA+TranslationA)
            // but we filter precisely in memory afterward, which is acceptable for typical export sizes
            var resourceNames = translationKeys.Select(k => k.ResourceName).Distinct().ToList();
            var translationNames = translationKeys.Select(k => k.TranslationName).Distinct().ToList();
            
            baseQuery = baseQuery.Where(t => 
                resourceNames.Contains(t.ResourceName) && 
                translationNames.Contains(t.TranslationName));
            
            var potentialMatches = await baseQuery.ToListAsync(cancellationToken);
            
            // Final filtering in memory to ensure exact key matches
            baseCultureTranslations = potentialMatches
                .Where(t => translationKeys.Contains(new TranslationKey(t.ResourceName, t.TranslationName)))
                .ToList();
        }

        var baseCultureExportDtos = baseCultureTranslations.ToExportDto();

        var exporter = _exporterFactory.GetExporter(request.Format);
        return await exporter.ExportAsync(translationExportDtos, new Dictionary<string, object>
        {
            { "TargetCulture", request.TargetCulture },
            { "BaseCulture", request.BaseCulture },
            { "BaseTranslations", baseCultureExportDtos }
        }, cancellationToken);
    }
}
