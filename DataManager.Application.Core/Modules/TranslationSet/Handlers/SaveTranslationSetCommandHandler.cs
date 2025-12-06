using System.Text.RegularExpressions;
using DataManager.Application.Contracts.Modules.TranslationSet;
using DataManager.Application.Core.Common;
using DataManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DataManager.Application.Core.Modules.TranslationSet.Handlers;

public class SaveTranslationSetCommandHandler : IRequestHandler<SaveTranslationSetCommand, Guid>
{
    private readonly DataManagerDbContext _context;
    private readonly TranslationSetsQueryService _queryService;

    // Compiled regex patterns for better performance
    private static readonly Regex NonAlphanumericPattern = new(@"[^a-z0-9-]", RegexOptions.Compiled);
    private static readonly Regex MultipleHyphensPattern = new(@"-+", RegexOptions.Compiled);

    public SaveTranslationSetCommandHandler(DataManagerDbContext context, TranslationSetsQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }


    public async Task<Guid> Handle(SaveTranslationSetCommand request, CancellationToken cancellationToken)
    {
        // Canonicalize the name to ensure it's URL-safe
        var canonicalName = CanonicalizeDataSetName(request.Name);
        
        TranslationSet? translationSet;

        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            // Update existing - use QueryService for consistent querying
            var options = new QueryOptions<TranslationSet, Guid>
            {
                IncludeFunc = q => q.Include(ds => ds.Includes)
            };

            translationSet = await _queryService.GetByIdAsync(
                request.Id.Value,
                options: options,
                cancellationToken: cancellationToken
            );

            if (translationSet == null)
            {
                throw new KeyNotFoundException($"TranslationSet with Id {request.Id} not found.");
            }

            translationSet.Name = canonicalName;
            translationSet.Description = request.Description;
            translationSet.Notes = request.Notes;
            translationSet.AllowedIdentityIds = request.AllowedIdentityIds.ToList();
            translationSet.AvailableCultures = request.AvailableCultures?.ToList();

            // Update includes
            var existingIncludes = translationSet.Includes.ToList();
            var newIncludeIds = request.IncludedTranslationSetIds.ToList();

            // Remove includes that are no longer in the list
            foreach (var include in existingIncludes)
            {
                if (!newIncludeIds.Contains(include.IncludedTranslationSetId))
                {
                    translationSet.Includes.Remove(include);
                }
            }

            // Add new includes
            var existingIncludeIds = existingIncludes.Select(i => i.IncludedTranslationSetId).ToList();
            foreach (var newIncludeId in newIncludeIds)
            {
                if (!existingIncludeIds.Contains(newIncludeId))
                {
                    translationSet.Includes.Add(new TranslationSetInclude
                    {
                        ParentTranslationSetId = translationSet.Id,
                        IncludedTranslationSetId = newIncludeId,
                        CreatedAt = DateTimeOffset.UtcNow
                    });
                }
            }
        }
        else
        {
            // Create new
            translationSet = new TranslationSet
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
            foreach (var includeId in request.IncludedTranslationSetIds)
            {
                translationSet.Includes.Add(new TranslationSetInclude
                {
                    ParentTranslationSetId = translationSet.Id,
                    IncludedTranslationSetId = includeId,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }

            _context.TranslationSets.Add(translationSet);
        }

        return translationSet.Id;
    }
    
    
    /// <summary>
    /// Converts a string to a canonical URL-safe format (lowercase alphanumeric and hyphens only).
    /// </summary>
    private static string CanonicalizeDataSetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("TranslationSet name cannot be empty.", nameof(name));
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
            throw new ArgumentException("TranslationSet name must contain at least one alphanumeric character.", nameof(name));
        }

        return canonical;
    }
}
