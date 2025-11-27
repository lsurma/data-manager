using InstanceManager.Application.Contracts.Modules.Translations;
using InstanceManager.Application.Core.Data;
using MediatR;

namespace InstanceManager.Application.Core.Modules.Translations.Handlers;

public class SaveTranslationCommandHandler : IRequestHandler<SaveTranslationCommand, Guid>
{
    private readonly InstanceManagerDbContext _context;
    private readonly TranslationsQueryService _queryService;

    public SaveTranslationCommandHandler(InstanceManagerDbContext context, TranslationsQueryService queryService)
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

            translation.InternalGroupName1 = request.InternalGroupName1;
            translation.InternalGroupName2 = request.InternalGroupName2;
            translation.ResourceName = request.ResourceName;
            translation.TranslationName = request.TranslationName;
            translation.CultureName = request.CultureName;
            translation.Content = request.Content;
            translation.ContentTemplate = request.ContentTemplate;
            translation.DataSetId = request.DataSetId;
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
                CreatedBy = string.Empty // Will be set by DbContext
            };

            _context.Translations.Add(translation);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return translation.Id;
    }
}
