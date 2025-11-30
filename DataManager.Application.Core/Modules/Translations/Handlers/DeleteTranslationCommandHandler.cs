using DataManager.Application.Contracts.Modules.Translations;
using DataManager.Application.Core.Data;
using MediatR;

namespace DataManager.Application.Core.Modules.Translations.Handlers;

public class DeleteTranslationCommandHandler : IRequestHandler<DeleteTranslationCommand, bool>
{
    private readonly DataManagerDbContext _context;
    private readonly TranslationsQueryService _queryService;

    public DeleteTranslationCommandHandler(DataManagerDbContext context, TranslationsQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<bool> Handle(DeleteTranslationCommand request, CancellationToken cancellationToken)
    {
        // GetByIdAsync applies authorization automatically via TranslationsQueryService
        // No need to pass query - service uses DefaultQuery
        var translation = await _queryService.GetByIdAsync<Translation>(
            request.Id,
            cancellationToken: cancellationToken);

        if (translation == null)
        {
            return false; // Not found or no access
        }

        _context.Translations.Remove(translation);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
