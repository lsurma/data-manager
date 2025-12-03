using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Application.Core.Data;
using MediatR;

namespace DataManager.Application.Core.Modules.Translations.Handlers;

public class SaveTranslationCommandHandler : IRequestHandler<SaveTranslationCommand, Guid>
{
    private readonly DataManagerDbContext _context;
    private readonly TranslationsQueryService _queryService;

    public SaveTranslationCommandHandler(DataManagerDbContext context, TranslationsQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<Guid> Handle(SaveTranslationCommand request, CancellationToken cancellationToken)
    {
        Translation? translation;

        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            // Update existing - GetByIdAsync applies authorization automatically
            // No need to pass query - service uses DefaultQuery
            translation = await _queryService.GetByIdAsync(
                request.Id.Value,
                cancellationToken: cancellationToken
            );

            if (translation == null)
            {
                throw new KeyNotFoundException($"Translation with Id {request.Id} not found or you don't have access to it.");
            }

            // Check if any data has changed (excluding version flags)
            bool hasChanges = translation.InternalGroupName1 != request.InternalGroupName1
                || translation.InternalGroupName2 != request.InternalGroupName2
                || translation.ResourceName != request.ResourceName
                || translation.TranslationName != request.TranslationName
                || translation.CultureName != request.CultureName
                || translation.Content != request.Content
                || translation.ContentTemplate != request.ContentTemplate
                || translation.DataSetId != request.DataSetId
                || translation.LayoutId != request.LayoutId
                || translation.SourceId != request.SourceId;

            // Create old version if there are changes AND the result will be a current (non-draft) version
            // Design decision: Only track version history for published versions, not drafts
            // - Draft-to-draft changes: No version history (working copy)
            // - Draft-to-published: No old version created (first publication)
            // - Published-to-published: Old version created (track published changes)
            // - Published-to-draft: No old version created (unpublishing)
            bool shouldCreateOldVersion = hasChanges && !request.IsDraftVersion && !translation.IsDraftVersion;
            
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

            // Update the translation with new values
            translation.InternalGroupName1 = request.InternalGroupName1;
            translation.InternalGroupName2 = request.InternalGroupName2;
            translation.ResourceName = request.ResourceName;
            translation.TranslationName = request.TranslationName;
            translation.CultureName = request.CultureName;
            translation.Content = request.Content;
            translation.ContentTemplate = request.ContentTemplate;
            translation.DataSetId = request.DataSetId;
            translation.LayoutId = request.LayoutId;
            translation.SourceId = request.SourceId;
            
            // Set version flags based on draft status
            if (request.IsDraftVersion)
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
        else
        {
            // Create new
            translation = new Translation
            {
                Id = Guid.NewGuid(),
                InternalGroupName1 = request.InternalGroupName1,
                InternalGroupName2 = request.InternalGroupName2,
                ResourceName = request.ResourceName,
                TranslationName = request.TranslationName,
                CultureName = request.CultureName,
                Content = request.Content,
                ContentTemplate = request.ContentTemplate,
                DataSetId = request.DataSetId,
                LayoutId = request.LayoutId,
                SourceId = request.SourceId,
                CreatedBy = string.Empty, // Will be set by DbContext
                IsCurrentVersion = !request.IsDraftVersion,
                IsDraftVersion = request.IsDraftVersion,
                IsOldVersion = false
            };

            _context.Translations.Add(translation);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return translation.Id;
    }
}
