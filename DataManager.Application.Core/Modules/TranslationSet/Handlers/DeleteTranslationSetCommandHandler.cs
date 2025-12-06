using DataManager.Application.Contracts.Modules.TranslationSet;
using DataManager.Application.Core.Data;
using MediatR;

namespace DataManager.Application.Core.Modules.TranslationSet.Handlers;

public class DeleteTranslationSetCommandHandler : IRequestHandler<DeleteTranslationSetCommand, bool>
{
    private readonly DataManagerDbContext _context;
    private readonly TranslationSetsQueryService _queryService;

    public DeleteTranslationSetCommandHandler(DataManagerDbContext context, TranslationSetsQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<bool> Handle(DeleteTranslationSetCommand request, CancellationToken cancellationToken)
    {
        var translationSet = await _queryService.GetByIdAsync(
            request.Id,
            cancellationToken: cancellationToken
        );

        if (translationSet == null)
        {
            return false;
        }

        _context.TranslationSets.Remove(translationSet);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
