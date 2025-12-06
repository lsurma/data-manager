using DataManager.Application.Contracts.Modules.TranslationsSet;
using DataManager.Application.Core.Data;
using MediatR;

namespace DataManager.Application.Core.Modules.TranslationsSet.Handlers;

public class DeleteTranslationsSetCommandHandler : IRequestHandler<DeleteTranslationsSetCommand, bool>
{
    private readonly DataManagerDbContext _context;
    private readonly TranslationsSetsQueryService _queryService;

    public DeleteTranslationsSetCommandHandler(DataManagerDbContext context, TranslationsSetsQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<bool> Handle(DeleteTranslationsSetCommand request, CancellationToken cancellationToken)
    {
        var translationsSet = await _queryService.GetByIdAsync(
            request.Id,
            cancellationToken: cancellationToken
        );

        if (translationsSet == null)
        {
            return false;
        }

        _context.TranslationsSets.Remove(translationsSet);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
