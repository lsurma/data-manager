using DataManager.Application.Contracts.Modules.Translations;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DataManager.Application.Core.Modules.Translations.Handlers;

public class ImportTranslationsCommandHandler : IRequestHandler<ImportTranslationsCommand, ImportTranslationsResult>
{
    private readonly IMediator _mediator;
    private readonly ILogger<ImportTranslationsCommandHandler> _logger;

    public ImportTranslationsCommandHandler(
        IMediator mediator, 
        ILogger<ImportTranslationsCommandHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<ImportTranslationsResult> Handle(ImportTranslationsCommand request, CancellationToken cancellationToken)
    {
        var result = new ImportTranslationsResult();

        foreach (var translation in request.Translations)
        {
            try
            {
                var saveCommand = new SaveTranslationCommand
                {
                    ResourceName = translation.ResourceName,
                    TranslationName = translation.TranslationName,
                    Content = translation.Content,
                    CultureName = translation.CultureName,
                    InternalGroupName1 = translation.InternalGroupName1,
                    InternalGroupName2 = translation.InternalGroupName2,
                    ContentTemplate = translation.ContentTemplate,
                    DataSetId = request.DataSetId
                };

                await _mediator.Send(saveCommand, cancellationToken);
                result.ImportedCount++;
            }
            catch (Exception ex)
            {
                result.FailedCount++;
                result.Errors.Add($"Failed to import translation '{translation.TranslationName}' ({translation.ResourceName}): {ex.Message}");
                _logger.LogError(ex, "Failed to import translation {TranslationName} ({ResourceName})", 
                    translation.TranslationName, translation.ResourceName);
            }
        }

        return result;
    }
}
