using System.Text.RegularExpressions;
using DataManager.Application.Contracts.Modules.TranslationsSet;
using DataManager.Application.Core.Common;
using DataManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DataManager.Application.Core.Modules.TranslationsSet.Handlers;

public class SaveTranslationsSetCommandHandler : IRequestHandler<SaveTranslationsSetCommand, Guid>
{
    private readonly DataManagerDbContext _context;
    private readonly TranslationsSetsQueryService _queryService;

    // Compiled regex patterns for better performance
    private static readonly Regex NonAlphanumericPattern = new(@"[^a-z0-9-]", RegexOptions.Compiled);
    private static readonly Regex MultipleHyphensPattern = new(@"-+", RegexOptions.Compiled);

    public SaveTranslationsSetCommandHandler(DataManagerDbContext context, TranslationsSetsQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }


    public async Task<Guid> Handle(SaveTranslationsSetCommand request, CancellationToken cancellationToken)
    {
        // Canonicalize the name to ensure it's URL-safe
        var canonicalName = CanonicalizeDataSetName(request.Name);
        
        TranslationsSet? translationsSet;

        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            // Update existing - use QueryService for consistent querying
            var options = new QueryOptions<TranslationsSet, Guid>
            {
                IncludeFunc = q => q.Include(ds => ds.Includes)
            };

            translationsSet = await _queryService.GetByIdAsync(
                request.Id.Value,
                options: options,
                cancellationToken: cancellationToken
            );

            if (translationsSet == null)
            {
                throw new KeyNotFoundException($"TranslationsSet with Id {request.Id} not found.");
            }

            translationsSet.Name = canonicalName;
            translationsSet.Description = request.Description;
            translationsSet.Notes = request.Notes;
            translationsSet.AllowedIdentityIds = request.AllowedIdentityIds.ToList();
            translationsSet.AvailableCultures = request.AvailableCultures?.ToList();

            // Update includes
            var existingIncludes = translationsSet.Includes.ToList();
            var newIncludeIds = request.IncludedTranslationsSetIds.ToList();

            // Remove includes that are no longer in the list
            foreach (var include in existingIncludes)
            {
                if (!newIncludeIds.Contains(include.IncludedTranslationsSetId))
                {
                    translationsSet.Includes.Remove(include);
                }
            }

            // Add new includes
            var existingIncludeIds = existingIncludes.Select(i => i.IncludedTranslationsSetId).ToList();
            foreach (var newIncludeId in newIncludeIds)
            {
                if (!existingIncludeIds.Contains(newIncludeId))
                {
                    translationsSet.Includes.Add(new TranslationsSetInclude
                    {
                        ParentTranslationsSetId = translationsSet.Id,
                        IncludedTranslationsSetId = newIncludeId,
                        CreatedAt = DateTimeOffset.UtcNow
                    });
                }
            }
        }
        else
        {
            // Create new
            translationsSet = new TranslationsSet
            {
                Id = Guid.NewGuid(),
                Name = canonicalName,
                Description = request.Description,
                Notes = request.Notes,
                AllowedIdentityIds = request.AllowedIdentityIds.ToList(),
                AvailableCultures = request.AvailableCultures?.ToList(),
                CreatedBy = string.Empty // Will be set by DbContext
            };

            // Add includes
            foreach (var includeId in request.IncludedTranslationsSetIds)
            {
                translationsSet.Includes.Add(new TranslationsSetInclude
                {
                    ParentTranslationsSetId = translationsSet.Id,
                    IncludedTranslationsSetId = includeId,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }

            _context.TranslationsSets.Add(translationsSet);
        }

        return translationsSet.Id;
    }
    
    
    /// <summary>
    /// Converts a string to a canonical URL-safe format (lowercase alphanumeric and hyphens only).
    /// </summary>
    private static string CanonicalizeDataSetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("TranslationsSet name cannot be empty.", nameof(name));
        }

        // Convert to lowercase
        var canonical = name.ToLowerInvariant();

        // Replace any non-alphanumeric characters (except hyphens) with hyphens
        canonical = NonAlphanumericPattern.Replace(canonical, "-");

        // Replace multiple consecutive hyphens with a single hyphen
        canonical = MultipleHyphensPattern.Replace(canonical, "-");

        // Remove leading and trailing hyphens
        canonical = canonical.Trim('-');

        // Ensure we have something left after canonicalization
        if (string.IsNullOrEmpty(canonical))
        {
            throw new ArgumentException("TranslationsSet name must contain at least one alphanumeric character.", nameof(name));
        }

        return canonical;
    }
}
