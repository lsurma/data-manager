using DataManager.Application.Contracts.Modules.TranslationsSet;
using DataManager.Application.Core.Common;
using DataManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DataManager.Application.Core.Modules.TranslationsSet.Handlers;

public class GetTranslationsSetByIdQueryHandler : IRequestHandler<GetTranslationsSetByIdQuery, TranslationsSetDto?>
{
    private readonly DataManagerDbContext _context;
    private readonly TranslationsSetsQueryService _queryService;

    public GetTranslationsSetByIdQueryHandler(DataManagerDbContext context, TranslationsSetsQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<TranslationsSetDto?> Handle(GetTranslationsSetByIdQuery request, CancellationToken cancellationToken)
    {
        var options = new QueryOptions<TranslationsSet, Guid>
        {
            AsNoTracking = true,
            IncludeFunc = q => q.Include(ds => ds.Includes)
        };

        var translationsSet = await _queryService.GetByIdAsync(
            request.Id,
            options: options,
            cancellationToken: cancellationToken
        );

        return translationsSet?.ToDto();
    }
}
