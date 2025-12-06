using DataManager.Application.Contracts.Modules.TranslationSet;
using DataManager.Application.Core.Common;
using DataManager.Application.Core.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DataManager.Application.Core.Modules.TranslationSet.Handlers;

public class GetTranslationSetByIdQueryHandler : IRequestHandler<GetTranslationSetByIdQuery, TranslationSetDto?>
{
    private readonly DataManagerDbContext _context;
    private readonly TranslationSetsQueryService _queryService;

    public GetTranslationSetByIdQueryHandler(DataManagerDbContext context, TranslationSetsQueryService queryService)
    {
        _context = context;
        _queryService = queryService;
    }

    public async Task<TranslationSetDto?> Handle(GetTranslationSetByIdQuery request, CancellationToken cancellationToken)
    {
        var options = new QueryOptions<TranslationSet, Guid>
        {
            AsNoTracking = true,
            IncludeFunc = q => q.Include(ds => ds.Includes)
        };

        var translationSet = await _queryService.GetByIdAsync(
            request.Id,
            options: options,
            cancellationToken: cancellationToken
        );

        return translationSet?.ToDto();
    }
}
