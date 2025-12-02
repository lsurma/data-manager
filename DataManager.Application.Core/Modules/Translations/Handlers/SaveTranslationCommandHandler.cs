using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Application.Core.Data;
using DataManager.Authentication.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DataManager.Application.Core.Modules.Translations.Handlers;

public class SaveTranslationCommandHandler : IRequestHandler<SaveTranslationCommand, Guid>
{
    private readonly DataManagerDbContext _context;
    private readonly TranslationsQueryService _queryService;
    private readonly IOptions<TranslationOptions> _options;
    private readonly ICurrentUserService _currentUserService;

    public SaveTranslationCommandHandler(
        DataManagerDbContext context,
        TranslationsQueryService queryService,
        IOptions<TranslationOptions> options,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _queryService = queryService;
        _options = options;
        _currentUserService = currentUserService;
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

            var originalValues = _context.Entry(translation).OriginalValues.Clone();

            var updatedTranslation = new Translation
            {
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
                DraftContent = request.DraftContent,
                CreatedBy = translation.CreatedBy
            };

            _context.Entry(translation).CurrentValues.SetValues(updatedTranslation);

            var hasChanges = _context.Entry(translation).Properties.Any(p => p.IsModified);

            if (hasChanges)
            {
                // Load existing old versions
                await _context.Entry(translation)
                    .Collection(t => t.OldVersions)
                    .LoadAsync(cancellationToken);

                var oldVersion = new TranslationVersion
                {
                    TranslationId = translation.Id,
                    InternalGroupName1 = (string)originalValues[nameof(Translation.InternalGroupName1)],
                    InternalGroupName2 = (string)originalValues[nameof(Translation.InternalGroupName2)],
                    ResourceName = (string)originalValues[nameof(Translation.ResourceName)],
                    TranslationName = (string)originalValues[nameof(Translation.TranslationName)],
                    CultureName = (string)originalValues[nameof(Translation.CultureName)],
                    Content = (string)originalValues[nameof(Translation.Content)],
                    ContentTemplate = (string)originalValues[nameof(Translation.ContentTemplate)],
                    DataSetId = (Guid?)originalValues[nameof(Translation.DataSetId)],
                    LayoutId = (Guid?)originalValues[nameof(Translation.LayoutId)],
                    SourceId = (Guid?)originalValues[nameof(Translation.SourceId)],
                    CreatedBy = _currentUserService.GetUserId() ?? "system"
                };
                translation.OldVersions.Add(oldVersion);

                if (translation.OldVersions.Count > _options.Value.MaxOldVersions)
                {
                    var versionsToRemove = translation.OldVersions
                        .OrderBy(v => v.CreatedAt)
                        .Take(translation.OldVersions.Count - _options.Value.MaxOldVersions)
                        .ToList();

                    foreach (var version in versionsToRemove)
                    {
                        _context.TranslationVersions.Remove(version);
                    }
                }
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
                CreatedBy = string.Empty // Will be set by DbContext
            };

            _context.Translations.Add(translation);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return translation.Id;
    }
}
