using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Application.Core.Common;
using DataManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataManager.Application.Core.Modules.Translations.Handlers;

/// <summary>
/// Handler for saving a single translation with versioning support.
/// This is a low-level handler used internally for single translation persistence.
/// Supports partial updates using Optional&lt;T&gt; properties.
/// </summary>
public class SaveSingleTranslationCommandHandler : IRequestHandler<SaveSingleTranslationCommand, Guid>
{
    private readonly DataManagerDbContext _context;
    private readonly TranslationsQueryService _queryService;
    private readonly ILogger<SaveSingleTranslationCommandHandler> _logger;

    public SaveSingleTranslationCommandHandler(
        DataManagerDbContext context, 
        TranslationsQueryService queryService,
        ILogger<SaveSingleTranslationCommandHandler> logger)
    {
        _context = context;
        _queryService = queryService;
        _logger = logger;
    }

    /// <summary>
    /// Tries to get a translation by ID from the local view (change tracker) first,
    /// falling back to a database query if not found.
    /// </summary>
    private async Task<Translation?> GetByIdWithLocalViewAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        // Try local view first (from change tracker) to avoid database hit if already loaded
        var translation = await _queryService.GetByIdAsync(
            id,
            options: new QueryOptions<Translation, Guid>
            {
                UseLocalView = true
            },
            cancellationToken: cancellationToken);

        // If not found in local view, query the database
        if (translation == null)
        {
            translation = await _queryService.GetByIdAsync(
                id,
                cancellationToken: cancellationToken);
        }

        return translation;
    }

    public async Task<Guid> Handle(SaveSingleTranslationCommand request, CancellationToken cancellationToken)
    {
        Translation? translation = null;
        
        // Extract ContentUpdatedAt value for easier access and validation
        DateTimeOffset? requestContentUpdatedAt = request.ContentUpdatedAt.IsSpecified && request.ContentUpdatedAt.Value.HasValue
            ? request.ContentUpdatedAt.Value.Value
            : null;

        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            // Update existing - GetByIdAsync applies authorization automatically
            translation = await GetByIdWithLocalViewAsync(request.Id.Value, cancellationToken);

            if (translation == null)
            {
                throw new KeyNotFoundException($"Translation with Id {request.Id} not found or you don't have access to it.");
            }
        }
        else
        {
            // Try to find existing translation by unique keys (ResourceName, TranslationName, CultureName)
            if (request.ResourceName.IsSpecified && request.TranslationName.IsSpecified && request.CultureName.IsSpecified && request.DataSetId.IsSpecified)
            {
                // Try local view first (from change tracker) to avoid database hit if already loaded
                var queryLocal = await _queryService.PrepareQueryAsync(
                    options: new QueryOptions<Translation, Guid>
                    {
                        UseLocalView = true
                    },
                    cancellationToken: cancellationToken);

                translation = queryLocal.FirstOrDefault(t =>
                    t.ResourceName == request.ResourceName.Value &&
                    t.TranslationName == request.TranslationName.Value &&
                    t.CultureName == request.CultureName.Value && 
                    t.DataSetId == request.DataSetId.Value &&
                    t.IsCurrentVersion
                );

                // If not found in local view, query the database
                if (translation == null)
                {
                    var query = await _queryService.PrepareQueryAsync(
                        options: new QueryOptions<Translation, Guid>
                        {
                            AsNoTracking = false // Database fallback: need tracking for updates
                        },
                        cancellationToken: cancellationToken);

                    translation = await query.FirstOrDefaultAsync(t =>
                        t.ResourceName == request.ResourceName.Value &&
                        t.TranslationName == request.TranslationName.Value &&
                        t.CultureName == request.CultureName.Value && 
                        t.DataSetId == request.DataSetId.Value &&
                        t.IsCurrentVersion,
                        cancellationToken
                    );
                }
            }
        }
        
        if(translation != null)
        {
            // Check if ContentUpdatedAt is provided and is older than the existing entity
            if (requestContentUpdatedAt.HasValue)
            {
                if (translation.ContentUpdatedAt.HasValue && 
                    requestContentUpdatedAt.Value < translation.ContentUpdatedAt.Value)
                {
                    _logger.LogError(
                        "Attempted to update translation {TranslationId} with older ContentUpdatedAt. " +
                        "Request ContentUpdatedAt: {RequestContentUpdatedAt}, " +
                        "Entity ContentUpdatedAt: {EntityContentUpdatedAt}. Update rejected.",
                        translation.Id,
                        requestContentUpdatedAt.Value,
                        translation.ContentUpdatedAt.Value);
                    
                    // Return the existing translation ID without updating
                    return translation.Id;
                }
            }

            // Check if any data has changed (excluding version flags)
            // Only check properties that are specified
            // Check if content specifically has changed
            bool contentHasChanged = 
                (request.Content.IsSpecified && translation.Content != request.Content.Value)
                || (request.ContentTemplate.IsSpecified && translation.ContentTemplate != request.ContentTemplate.Value);

            // Determine the target draft status (use current if not specified)
            bool targetIsDraftVersion = request.IsDraftVersion.IsSpecified 
                ? request.IsDraftVersion.Value 
                : translation.IsDraftVersion;

            // Create old version if there are changes AND the result will be a current (non-draft) version
            // Design decision: Only track version history for published versions, not drafts
            // - Draft-to-draft changes: No version history (working copy)
            // - Draft-to-published: No old version created (first publication)
            // - Published-to-published: Old version created (track published changes)
            // - Published-to-draft: No old version created (unpublishing)
            bool shouldCreateOldVersion = contentHasChanged && !targetIsDraftVersion && !translation.IsDraftVersion;

            if (shouldCreateOldVersion)
            {
                // Create a copy of the current version and mark it as old
                var oldVersion = new Translation
                {
                    Id = Guid.NewGuid(),
                    InternalGroupName1 = translation.InternalGroupName1,
                    InternalGroupName2 = translation.InternalGroupName2,
                    ResourceName = translation.ResourceName,
                    TranslationName = translation.TranslationName,
                    CultureName = translation.CultureName,
                    Content = translation.Content,
                    ContentTemplate = translation.ContentTemplate,
                    ContentUpdatedAt = translation.ContentUpdatedAt,
                    DataSetId = translation.DataSetId,
                    LayoutId = translation.LayoutId,
                    SourceId = translation.SourceId,
                    OriginalTranslationId = translation.Id,
                    IsCurrentVersion = false,
                    IsDraftVersion = false,
                    IsOldVersion = true,
                    CreatedBy = translation.CreatedBy
                };

                _context.Translations.Add(oldVersion);
            }

            // Update only the specified properties
            translation.InternalGroupName1 = request.InternalGroupName1.GetValueOrDefault(translation.InternalGroupName1);
            translation.InternalGroupName2 = request.InternalGroupName2.GetValueOrDefault(translation.InternalGroupName2);
            translation.ResourceName = request.ResourceName.GetValueOrDefault(translation.ResourceName);
            translation.TranslationName = request.TranslationName.GetValueOrDefault(translation.TranslationName);            
            translation.CultureName = request.CultureName.GetValueOrDefault(translation.CultureName);
            
            // Update content properties and ContentUpdatedAt together when content changes
            translation.Content = request.Content.GetValueOrDefault(translation.Content);
            translation.ContentTemplate = request.ContentTemplate.GetValueOrDefault(translation.ContentTemplate);
            
            if (contentHasChanged)
            {
                // Use provided ContentUpdatedAt if specified (and it passed validation), otherwise use current time
                translation.ContentUpdatedAt = requestContentUpdatedAt ?? DateTimeOffset.UtcNow;
            }
            else if (requestContentUpdatedAt.HasValue)
            {
                // If ContentUpdatedAt is explicitly provided but content hasn't changed,
                // still update the timestamp (useful for external sync scenarios where the timestamp
                // needs to be preserved even if the content appears unchanged)
                translation.ContentUpdatedAt = requestContentUpdatedAt.Value;
            }
            
            translation.DataSetId = request.DataSetId.GetValueOrDefault(translation.DataSetId);
            translation.LayoutId = request.LayoutId.GetValueOrDefault(translation.LayoutId);
            translation.SourceId = request.SourceId.GetValueOrDefault(translation.SourceId);

            // Set version flags based on draft status
            if (request.IsDraftVersion.IsSpecified)
            {
                if (request.IsDraftVersion.Value)
                {
                    translation.IsCurrentVersion = false;
                    translation.IsDraftVersion = true;
                    translation.IsOldVersion = false;
                }
                else
                {
                    translation.IsCurrentVersion = true;
                    translation.IsDraftVersion = false;
                    translation.IsOldVersion = false;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return translation.Id;
        }
        else
        {
            if (!request.ResourceName.IsSpecified || string.IsNullOrEmpty(request.ResourceName.Value))
                throw new ArgumentException("ResourceName is required when creating a new translation.");
            
            if (!request.TranslationName.IsSpecified || string.IsNullOrEmpty(request.TranslationName.Value))
                throw new ArgumentException("TranslationName is required when creating a new translation.");
            
            if (!request.CultureName.IsSpecified || string.IsNullOrEmpty(request.CultureName.Value))
                throw new ArgumentException("CultureName is required when creating a new translation.");
            
            if(!request.DataSetId.IsSpecified || !request.DataSetId.Value.HasValue)
                throw new ArgumentException("DataSetId is required when creating a new translation.");
            
            translation = new Translation
            {
                Id = Guid.NewGuid(),
                InternalGroupName1 = request.InternalGroupName1.GetValueOrDefault(null),
                InternalGroupName2 = request.InternalGroupName2.GetValueOrDefault(null),
                ResourceName = request.ResourceName.Value,
                TranslationName = request.TranslationName.Value,
                CultureName = request.CultureName.Value,
                Content = request.Content.Value,
                ContentTemplate = request.ContentTemplate.GetValueOrDefault(null),
                ContentUpdatedAt = requestContentUpdatedAt ?? DateTimeOffset.UtcNow,
                DataSetId = request.DataSetId.GetValueOrDefault(null),
                LayoutId = request.LayoutId.GetValueOrDefault(null),
                SourceId = request.SourceId.GetValueOrDefault(null),
                CreatedBy = string.Empty, // Will be set by DbContext
                IsCurrentVersion = !request.IsDraftVersion.GetValueOrDefault(false),
                IsDraftVersion = request.IsDraftVersion.GetValueOrDefault(false),
                IsOldVersion = false
            };

            _context.Translations.Add(translation);
            await _context.SaveChangesAsync(cancellationToken);

            return translation.Id;
        }
    }
}
