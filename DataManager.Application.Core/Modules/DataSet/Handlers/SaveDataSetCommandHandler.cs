using System.Text.RegularExpressions;
using DataManager.Application.Contracts.Modules.DataSet;
using DataManager.Application.Core.Common;
using DataManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DataManager.Application.Core.Modules.DataSet.Handlers;

public class SaveDataSetCommandHandler : IRequestHandler<SaveDataSetCommand, Guid>
{
    private readonly DataManagerDbContext _context;
    private readonly DataSetsQueryService _queryService;

    public SaveDataSetCommandHandler(DataManagerDbContext context, DataSetsQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
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
        canonical = Regex.Replace(canonical, @"[^a-z0-9-]", "-");

        // Replace multiple consecutive hyphens with a single hyphen
        canonical = Regex.Replace(canonical, @"-+", "-");

        // Remove leading and trailing hyphens
        canonical = canonical.Trim('-');

        // Ensure we have something left after canonicalization
        if (string.IsNullOrWhiteSpace(canonical))
        {
            throw new ArgumentException("DataSet name must contain at least one alphanumeric character.", nameof(name));
        }

        return canonical;
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

        await _context.SaveChangesAsync(cancellationToken);

        return dataSet.Id;
    }
}
