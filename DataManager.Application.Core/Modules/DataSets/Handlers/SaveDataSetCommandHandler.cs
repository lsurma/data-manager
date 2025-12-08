using DataManager.Application.Contracts.Modules.DataSets;
using DataManager.Application.Core.Common;
using DataManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace DataManager.Application.Core.Modules.DataSets.Handlers;

public class SaveDataSetCommandHandler : IRequestHandler<SaveDataSetCommand, Guid>
{
    private readonly DataManagerDbContext _context;
    private readonly DataSetsQueryService _queryService;

    // Compiled regex patterns for better performance
    private static readonly Regex NonAlphanumericPattern = new(@"[^a-z0-9-]", RegexOptions.Compiled);
    private static readonly Regex MultipleHyphensPattern = new(@"-+", RegexOptions.Compiled);

    public SaveDataSetCommandHandler(DataManagerDbContext context, DataSetsQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }


    public async Task<Guid> Handle(SaveDataSetCommand request, CancellationToken cancellationToken)
    {
        // Canonicalize the name to ensure it's URL-safe
        var canonicalName = CanonicalizeDataSetName(request.Name);
        
        DataSet? dataSet;

        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            // Update existing - use QueryService for consistent querying
            var options = new QueryOptions<DataSet, Guid>
            {
                IncludeFunc = q => q.Include(ds => ds.Includes)
            };

            dataSet = await _queryService.GetByIdAsync(
                request.Id.Value,
                options: options,
                cancellationToken: cancellationToken
            );

            if (dataSet == null)
            {
                throw new KeyNotFoundException($"DataSet with Id {request.Id} not found.");
            }

            dataSet.Name = canonicalName;
            dataSet.Description = request.Description;
            dataSet.Notes = request.Notes;
            dataSet.AllowedIdentityIds = request.AllowedIdentityIds.ToList();
            dataSet.AvailableCultures = request.AvailableCultures.ToList();
            dataSet.SecretKey = request.SecretKey;
            dataSet.WebhookUrls = request.WebhookUrls
                .Where(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .Select(url => new Uri(url))
                .ToList();

            // Update includes
            var existingIncludes = dataSet.Includes.ToList();
            var newIncludeIds = request.IncludedDataSetIds.ToList();

            // Remove includes that are no longer in the list
            foreach (var include in existingIncludes)
            {
                if (!newIncludeIds.Contains(include.IncludedDataSetId))
                {
                    dataSet.Includes.Remove(include);
                }
            }

            // Add new includes
            var existingIncludeIds = existingIncludes.Select(i => i.IncludedDataSetId).ToList();
            foreach (var newIncludeId in newIncludeIds)
            {
                if (!existingIncludeIds.Contains(newIncludeId))
                {
                    dataSet.Includes.Add(new DataSetInclude
                    {
                        ParentDataSetId = dataSet.Id,
                        IncludedDataSetId = newIncludeId,
                        CreatedAt = DateTimeOffset.UtcNow
                    });
                }
            }
        }
        else
        {
            // Create new
            dataSet = new DataSet
            {
                Id = Guid.NewGuid(),
                Name = canonicalName,
                Description = request.Description,
                Notes = request.Notes,
                AllowedIdentityIds = request.AllowedIdentityIds.ToList(),
                AvailableCultures = request.AvailableCultures.ToList(),
                SecretKey = request.SecretKey,
                WebhookUrls = request.WebhookUrls
                    .Where(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                    .Select(url => new Uri(url))
                    .ToList(),
                CreatedBy = string.Empty // Will be set by DbContext
            };

            // Add includes
            foreach (var includeId in request.IncludedDataSetIds)
            {
                dataSet.Includes.Add(new DataSetInclude
                {
                    ParentDataSetId = dataSet.Id,
                    IncludedDataSetId = includeId,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }

            _context.DataSets.Add(dataSet);
        }

        return dataSet.Id;
    }
    
    
    /// <summary>
    /// Converts a string to a canonical URL-safe format (lowercase alphanumeric and hyphens only).
    /// </summary>
    private static string CanonicalizeDataSetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("DataSet name cannot be empty.", nameof(name));
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
            throw new ArgumentException("DataSet name must contain at least one alphanumeric character.", nameof(name));
        }

        return canonical;
    }
}
